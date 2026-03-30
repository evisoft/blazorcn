// BlazorCN JS Interop — minimal behaviors that CSS can't handle

/** @type {Map<string, AbortController|{abort: Function, previouslyFocused?: Element}>} */
const cleanupMap = new Map();

/**
 * Traps focus within an element (for modals/dialogs).
 * @param {HTMLElement} element
 * @param {string} id - unique ID for cleanup
 */
export function trapFocus(element, id) {
    if (!element) return;
    const previouslyFocused = document.activeElement;
    cleanup(id);

    const focusKey = id + ':focus';
    const controller = new AbortController();
    cleanupMap.set(focusKey, { abort: () => controller.abort(), previouslyFocused });

    const focusable = getFocusableElements(element);
    if (focusable.length === 0) return;

    focusable[0].focus();

    element.addEventListener('keydown', (e) => {
        if (e.key !== 'Tab') return;

        const currentFocusable = getFocusableElements(element);
        const first = currentFocusable[0];
        const last = currentFocusable[currentFocusable.length - 1];

        if (e.shiftKey) {
            if (document.activeElement === first) {
                e.preventDefault();
                last.focus();
            }
        } else {
            if (document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        }
    }, { signal: controller.signal });
}

/**
 * Detects clicks outside an element.
 * @param {HTMLElement} element
 * @param {string} id - unique ID for cleanup
 * @param {object} dotnetRef - .NET object reference for callback
 * @param {string} methodName - .NET method to invoke
 */
export function onOutsideClick(element, id, dotnetRef, methodName) {
    if (!element) return;
    cleanup(id);

    const controller = new AbortController();
    cleanupMap.set(id, controller);

    setTimeout(() => {
        if (controller.signal.aborted) return;
        document.addEventListener('pointerdown', (e) => {
            if (!element.isConnected) return;
            if (!element.contains(e.target)) {
                dotnetRef.invokeMethodAsync(methodName);
            }
        }, { signal: controller.signal });
    }, 0);
}

let _scrollLockCount = 0;
let _savedScrollY = 0;

/**
 * Locks body scroll (for modals).
 * Uses reference counting so nested modals don't break scroll restore.
 * @param {string} id
 */
export function lockScroll(id) {
    const scrollKey = id + ':scroll';
    if (cleanupMap.has(scrollKey)) return; // already locked by this id
    if (_scrollLockCount === 0) {
        _savedScrollY = window.scrollY;
        document.body.style.position = 'fixed';
        document.body.style.top = `-${_savedScrollY}px`;
        document.body.style.left = '0';
        document.body.style.right = '0';
        document.body.style.overflow = 'hidden';
    }
    _scrollLockCount++;
    cleanupMap.set(scrollKey, { abort: () => {
        _scrollLockCount = Math.max(0, _scrollLockCount - 1);
        if (_scrollLockCount === 0) unlockScrollInternal(_savedScrollY);
    }});
}

function unlockScrollInternal(scrollY) {
    document.body.style.position = '';
    document.body.style.top = '';
    document.body.style.left = '';
    document.body.style.right = '';
    document.body.style.overflow = '';
    window.scrollTo(0, scrollY);
}

/**
 * Cleans up event listeners/state for a given ID.
 * @param {string} id
 */
export function cleanup(id) {
    // Process scroll before focus: restore scroll position before restoring focus.
    // Process base id last for non-suffixed callers (onOutsideClick, createFloating).
    for (const key of [id + ':scroll', id + ':focus', id]) {
        const existing = cleanupMap.get(key);
        if (existing) {
            if (typeof existing.abort === 'function') existing.abort();
            if (existing.previouslyFocused && existing.previouslyFocused.isConnected) {
                existing.previouslyFocused.focus();
            }
            cleanupMap.delete(key);
        }
    }
}

// --- Floating Positioning ---

/** @type {Map<string, { reference: HTMLElement, floating: HTMLElement, update: Function }>} */
const floatingMap = new Map();

/**
 * Creates a floating element positioned relative to a reference element.
 * @param {HTMLElement} reference - The trigger/anchor element
 * @param {HTMLElement} floating - The floating content element
 * @param {string} id - Unique ID for cleanup
 * @param {object} options - { side, sideOffset, align, alignOffset }
 * @returns {object} - { side: actualSide }
 */
export function createFloating(reference, floating, id, options) {
    if (!reference || !floating) return 'bottom';
    destroyFloating(id);

    const opts = {
        side: options?.side ?? 'bottom',
        sideOffset: options?.sideOffset ?? 4,
        align: options?.align ?? 'center',
        alignOffset: options?.alignOffset ?? 0
    };

    const update = () => computePosition(reference, floating, opts);
    const actualSide = update();

    const controller = new AbortController();
    window.addEventListener('scroll', update, { signal: controller.signal, passive: true, capture: true });
    window.addEventListener('resize', update, { signal: controller.signal, passive: true });

    floatingMap.set(id, { reference, floating, update });
    cleanupMap.set(id, controller);

    return actualSide;
}

/**
 * Re-calculates the position of a floating element.
 * @param {string} id
 */
export function updateFloating(id) {
    const entry = floatingMap.get(id);
    if (entry) {
        entry.update();
    }
}

/**
 * Destroys a floating element and cleans up event listeners.
 * @param {string} id
 */
export function destroyFloating(id) {
    cleanup(id);
    floatingMap.delete(id);
}

/**
 * Computes and applies position for a floating element relative to a reference.
 * Handles auto-flip when the element overflows the viewport.
 * @param {HTMLElement} reference
 * @param {HTMLElement} floating
 * @param {object} options
 * @returns {string} actual side used
 */
function computePosition(reference, floating, options) {
    const refRect = reference.getBoundingClientRect();
    const rawFloatRect = floating.getBoundingClientRect();

    // If the floating element is hidden (width/height = 0), use scrollWidth/scrollHeight as fallbacks
    const floatRect = {
        top: rawFloatRect.top,
        left: rawFloatRect.left,
        bottom: rawFloatRect.bottom,
        right: rawFloatRect.right,
        width: rawFloatRect.width === 0 ? floating.scrollWidth : rawFloatRect.width,
        height: rawFloatRect.height === 0 ? floating.scrollHeight : rawFloatRect.height
    };

    const viewportW = window.innerWidth;
    const viewportH = window.innerHeight;

    let { side, sideOffset, align, alignOffset } = options;

    // Calculate position for a given side
    function calcForSide(s) {
        let top = 0;
        let left = 0;

        switch (s) {
            case 'top':
                top = refRect.top - floatRect.height - sideOffset;
                left = calcAlignHorizontal(refRect, floatRect, align, alignOffset);
                break;
            case 'bottom':
                top = refRect.bottom + sideOffset;
                left = calcAlignHorizontal(refRect, floatRect, align, alignOffset);
                break;
            case 'left':
                left = refRect.left - floatRect.width - sideOffset;
                top = calcAlignVertical(refRect, floatRect, align, alignOffset);
                break;
            case 'right':
                left = refRect.right + sideOffset;
                top = calcAlignVertical(refRect, floatRect, align, alignOffset);
                break;
        }
        return { top, left };
    }

    let pos = calcForSide(side);
    let actualSide = side;

    // Auto-flip if overflowing viewport
    if (overflows(pos, floatRect, viewportW, viewportH)) {
        const opposite = getOppositeSide(side);
        const altPos = calcForSide(opposite);
        if (!overflows(altPos, floatRect, viewportW, viewportH)) {
            pos = altPos;
            actualSide = opposite;
        }
        // If both overflow, keep original side
    }

    // CSS transforms (and other properties) create a new containing block for
    // fixed-positioned descendants. When that happens, position:fixed coordinates
    // are relative to the ancestor, not the viewport. Adjust accordingly.
    const cb = getContainingBlock(floating);
    if (cb) {
        const cbRect = cb.getBoundingClientRect();
        pos.top -= cbRect.top;
        pos.left -= cbRect.left;
    }

    // Apply position using fixed positioning (relative to viewport)
    floating.style.position = 'fixed';
    floating.style.top = `${pos.top}px`;
    floating.style.left = `${pos.left}px`;
    floating.style.margin = '0';
    floating.setAttribute('data-side', actualSide);

    return actualSide;
}

function calcAlignHorizontal(refRect, floatRect, align, alignOffset) {
    switch (align) {
        case 'start':
            return refRect.left + alignOffset;
        case 'end':
            return refRect.right - floatRect.width + alignOffset;
        case 'center':
        default:
            return refRect.left + (refRect.width - floatRect.width) / 2 + alignOffset;
    }
}

function calcAlignVertical(refRect, floatRect, align, alignOffset) {
    switch (align) {
        case 'start':
            return refRect.top + alignOffset;
        case 'end':
            return refRect.bottom - floatRect.height + alignOffset;
        case 'center':
        default:
            return refRect.top + (refRect.height - floatRect.height) / 2 + alignOffset;
    }
}

function overflows(pos, floatRect, viewportW, viewportH) {
    return pos.top < 0
        || pos.left < 0
        || pos.top + floatRect.height > viewportH
        || pos.left + floatRect.width > viewportW;
}

function getOppositeSide(side) {
    switch (side) {
        case 'top': return 'bottom';
        case 'bottom': return 'top';
        case 'left': return 'right';
        case 'right': return 'left';
        default: return 'bottom';
    }
}

/**
 * Finds the nearest ancestor that creates a containing block for fixed-positioned
 * elements (CSS transforms, perspective, filter, etc. all trigger this).
 * Returns null when the containing block is the viewport (normal case).
 */
function getContainingBlock(element) {
    let parent = element.parentElement;
    while (parent && parent !== document.body && parent !== document.documentElement) {
        const s = getComputedStyle(parent);
        if (s.transform !== 'none'
            || (s.translate && s.translate !== 'none')
            || (s.rotate && s.rotate !== 'none')
            || (s.scale && s.scale !== 'none')
            || s.perspective !== 'none'
            || s.willChange === 'transform' || s.willChange === 'perspective'
            || (s.filter && s.filter !== 'none')
            || (s.backdropFilter && s.backdropFilter !== 'none')) {
            return parent;
        }
        parent = parent.parentElement;
    }
    return null;
}

// --- Keyboard Navigation ---

/**
 * Sets up keyboard navigation for a menu/list container.
 * Arrow up/down (or left/right for horizontal) navigates between items.
 * Home/End jumps to first/last.
 * Escape invokes .NET callback.
 * Enter/Space invokes click on focused item.
 * @param {HTMLElement} container
 * @param {string} id - for cleanup
 * @param {object} dotnetRef - .NET reference for escape callback
 * @param {string} escapeMethodName - method to call on escape
 * @param {object} options - { selector: string, orientation: 'vertical'|'horizontal'|'both' }
 */
export function setupKeyboardNavigation(container, id, dotnetRef, escapeMethodName, options) {
    if (!container) return;
    cleanupKeyboardNavigation(id);

    const selector = options?.selector ?? '[data-menu-item]';
    const orientation = options?.orientation ?? 'vertical';
    const controller = new AbortController();
    cleanupMap.set(id + ':kbd', controller);

    container.addEventListener('keydown', (e) => {
        const items = getNavigableItems(container, selector);
        if (items.length === 0) return;

        const currentIndex = items.indexOf(document.activeElement);
        const prevKey = orientation === 'horizontal' ? 'ArrowLeft' : 'ArrowUp';
        const nextKey = orientation === 'horizontal' ? 'ArrowRight' : 'ArrowDown';

        // For 'both' orientation, accept all arrow keys
        const isPrev = e.key === prevKey || (orientation === 'both' && (e.key === 'ArrowUp' || e.key === 'ArrowLeft'));
        const isNext = e.key === nextKey || (orientation === 'both' && (e.key === 'ArrowDown' || e.key === 'ArrowRight'));

        if (isPrev) {
            e.preventDefault();
            const startIdx = currentIndex === -1 ? items.length : currentIndex;
            const nextIdx = findPrevEnabled(items, startIdx);
            if (nextIdx >= 0) items[nextIdx].focus();
        } else if (isNext) {
            e.preventDefault();
            const nextIdx = findNextEnabled(items, currentIndex);
            if (nextIdx >= 0) items[nextIdx].focus();
        } else if (e.key === 'Home') {
            e.preventDefault();
            const firstEnabled = findNextEnabled(items, -1);
            if (firstEnabled >= 0) items[firstEnabled].focus();
        } else if (e.key === 'End') {
            e.preventDefault();
            const lastEnabled = findPrevEnabled(items, items.length);
            if (lastEnabled >= 0) items[lastEnabled].focus();
        } else if (e.key === 'Escape') {
            e.preventDefault();
            if (dotnetRef && escapeMethodName) {
                dotnetRef.invokeMethodAsync(escapeMethodName);
            }
        } else if (e.key === 'Enter' || e.key === ' ') {
            if (currentIndex >= 0 && !isItemDisabled(items[currentIndex])) {
                e.preventDefault();
                items[currentIndex].click();
            }
        }
    }, { signal: controller.signal });

    // Focus the first enabled item
    const items = getNavigableItems(container, selector);
    const firstEnabled = findNextEnabled(items, -1);
    if (firstEnabled >= 0) items[firstEnabled].focus();
}

/**
 * Cleans up keyboard navigation for a given ID.
 * @param {string} id
 */
export function cleanupKeyboardNavigation(id) {
    cleanup(id + ':kbd');
}

/**
 * Gets all items matching the selector within a container. Disabled items are handled during navigation.
 * @param {HTMLElement} container
 * @param {string} selector
 * @returns {HTMLElement[]}
 */
function getNavigableItems(container, selector) {
    return [...container.querySelectorAll(selector)];
}

/**
 * Checks if an element is disabled.
 * @param {HTMLElement} el
 * @returns {boolean}
 */
function isItemDisabled(el) {
    return el.hasAttribute('data-disabled') || el.hasAttribute('disabled') || el.getAttribute('aria-disabled') === 'true';
}

/**
 * Finds the next enabled item index, wrapping around.
 * @param {HTMLElement[]} items
 * @param {number} currentIndex
 * @returns {number} index of next enabled item, or -1 if none
 */
function findNextEnabled(items, currentIndex) {
    for (let i = 1; i <= items.length; i++) {
        const idx = (currentIndex + i) % items.length;
        if (!isItemDisabled(items[idx])) return idx;
    }
    return -1;
}

/**
 * Finds the previous enabled item index, wrapping around.
 * @param {HTMLElement[]} items
 * @param {number} currentIndex
 * @returns {number} index of previous enabled item, or -1 if none
 */
function findPrevEnabled(items, currentIndex) {
    for (let i = 1; i <= items.length; i++) {
        const idx = (currentIndex - i + items.length) % items.length;
        if (!isItemDisabled(items[idx])) return idx;
    }
    return -1;
}

// --- Utilities ---

/**
 * Gets all focusable elements within a container.
 * @param {HTMLElement} container
 * @returns {HTMLElement[]}
 */
function getFocusableElements(container) {
    return [...container.querySelectorAll(
        'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
    )];
}
