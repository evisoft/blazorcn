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

    const viewportW = window.innerWidth;
    const viewportH = window.innerHeight;

    let { side, sideOffset, align, alignOffset } = options;

    // Expose the reference (trigger) width so width-matching popovers (Select,
    // DropdownMenu, Combobox) can size themselves via `w-(--anchor-width)`.
    // Mirrors Base UI's `--anchor-width`; `--cn-trigger-width` kept for min-w
    // consumers. MUST be set BEFORE measuring the floating rect, otherwise the
    // position is computed for the natural width and goes stale once the width
    // var kicks in (content appears shifted sideways on first open).
    floating.style.setProperty('--cn-trigger-width', `${refRect.width}px`);
    floating.style.setProperty('--anchor-width', `${refRect.width}px`);

    // Available space between the anchor and the viewport edge on a given side
    // (mirrors Base UI's --available-height/--available-width). Read by
    // `max-h-(--available-height)` clamps so tall dropdowns scroll instead of
    // overflowing the viewport. Set before measuring so the clamp is in effect.
    const VIEWPORT_PAD = 10;
    function setAvailableSpace(s) {
        let availH, availW;
        switch (s) {
            case 'top': availH = refRect.top - sideOffset - VIEWPORT_PAD; availW = viewportW - 2 * VIEWPORT_PAD; break;
            case 'left': availW = refRect.left - sideOffset - VIEWPORT_PAD; availH = viewportH - 2 * VIEWPORT_PAD; break;
            case 'right': availW = viewportW - refRect.right - sideOffset - VIEWPORT_PAD; availH = viewportH - 2 * VIEWPORT_PAD; break;
            case 'bottom':
            default: availH = viewportH - refRect.bottom - sideOffset - VIEWPORT_PAD; availW = viewportW - 2 * VIEWPORT_PAD; break;
        }
        floating.style.setProperty('--available-height', `${Math.max(0, Math.round(availH))}px`);
        floating.style.setProperty('--available-width', `${Math.max(0, Math.round(availW))}px`);
    }
    setAvailableSpace(side);

    // Measure via offsetWidth/offsetHeight (layout size): getBoundingClientRect
    // is skewed by the open animation's scale transform mid-flight, producing a
    // few px of misposition. Falls back to scroll size if hidden.
    function measure() {
        return {
            width: floating.offsetWidth || floating.scrollWidth,
            height: floating.offsetHeight || floating.scrollHeight
        };
    }
    let floatRect = measure();

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

    // Auto-flip if overflowing viewport. Available-space clamps are re-applied
    // and the element re-measured per candidate side, since the clamp changes
    // the element's height.
    if (overflows(pos, floatRect, viewportW, viewportH)) {
        const opposite = getOppositeSide(side);
        setAvailableSpace(opposite);
        floatRect = measure();
        const altPos = calcForSide(opposite);
        if (!overflows(altPos, floatRect, viewportW, viewportH)) {
            pos = altPos;
            actualSide = opposite;
        } else {
            // Both overflow: keep original side; restore its clamp and position.
            setAvailableSpace(side);
            floatRect = measure();
            pos = calcForSide(side);
        }
    }

    // Anchor the open/close zoom animation to the trigger edge, mirroring
    // Base UI's --transform-origin (read via `origin-(--transform-origin)`).
    let originX, originY;
    if (actualSide === 'top' || actualSide === 'bottom') {
        originY = actualSide === 'top' ? 'bottom' : 'top';
        originX = align === 'start' ? 'left' : align === 'end' ? 'right' : 'center';
    } else {
        originX = actualSide === 'left' ? 'right' : 'left';
        originY = align === 'start' ? 'top' : align === 'end' ? 'bottom' : 'center';
    }
    floating.style.setProperty('--transform-origin', `${originX} ${originY}`);

    // Viewport coordinates, kept for arrow math before the containing-block shift.
    const viewportPos = { top: pos.top, left: pos.left };

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

    // Position the arrow (tooltip) on the resolved side, centered on the reference.
    positionArrow(floating, refRect, actualSide, viewportPos);

    return actualSide;
}

/**
 * Positions a floating element's arrow (if present) so it points at the reference,
 * centered along the shared edge and clamped to stay within the floating box.
 * The arrow is a square rotated 45deg, straddling the edge so its inner half is
 * hidden behind the same-colored content and its outer corner forms the pointer.
 * No-ops when the floating element has no arrow (only tooltips render one).
 * @param {HTMLElement} floating
 * @param {DOMRect} refRect - reference's viewport rect
 * @param {string} side - resolved side (top|bottom|left|right)
 * @param {object} pos - floating element's viewport position ({top, left}),
 *   used instead of getBoundingClientRect which is skewed mid-animation
 */
function positionArrow(floating, refRect, side, pos) {
    const arrow = floating.querySelector('[data-slot="tooltip-arrow"]');
    if (!arrow) return;

    const fWidth = floating.offsetWidth;
    const fHeight = floating.offsetHeight;
    const size = arrow.offsetWidth || 10;
    const half = size / 2;
    const pad = 6; // keep the arrow off the rounded corners

    if (side === 'top' || side === 'bottom') {
        const refCenterX = refRect.left + refRect.width / 2;
        let x = refCenterX - pos.left - half;
        x = Math.max(pad, Math.min(x, fWidth - size - pad));
        arrow.style.left = `${x}px`;
        arrow.style.top = side === 'top' ? '100%' : '0px';
        arrow.style.transform = 'translateY(-50%) rotate(45deg)';
    } else {
        const refCenterY = refRect.top + refRect.height / 2;
        let y = refCenterY - pos.top - half;
        y = Math.max(pad, Math.min(y, fHeight - size - pad));
        arrow.style.top = `${y}px`;
        arrow.style.left = side === 'left' ? '100%' : '0px';
        arrow.style.transform = 'translateX(-50%) rotate(45deg)';
    }
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
    // autoFocus: focus the first item on setup and restore focus to the pre-open
    // element on teardown (menu behavior). Pass false for persistent widgets
    // (e.g. a tabs list) that must not steal focus on mount.
    const autoFocus = options?.autoFocus ?? true;
    const previouslyFocused = document.activeElement;
    const controller = new AbortController();
    cleanupMap.set(id + ':kbd', {
        abort: () => {
            controller.abort();
            // Return focus to the trigger when the menu closes (Radix behavior).
            // Skipped if focus already moved to another element — e.g. the user
            // closed by clicking into an input — so we don't steal it.
            const active = document.activeElement;
            if (autoFocus
                && previouslyFocused && previouslyFocused.isConnected
                && (active === document.body || active === document.documentElement
                    || container.contains(active))) {
                previouslyFocused.focus();
            }
        }
    });

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

    // Focus the first enabled item (menus only — persistent widgets keep focus)
    if (autoFocus) {
        const items = getNavigableItems(container, selector);
        const firstEnabled = findNextEnabled(items, -1);
        if (firstEnabled >= 0) items[firstEnabled].focus();
    }
}

/**
 * Returns an element's text content — used to derive a command item's filter
 * text when no explicit Value parameter is supplied.
 * @param {HTMLElement} element
 * @returns {string}
 */
export function getTextContent(element) {
    return element?.textContent?.trim() ?? '';
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

// --- Scroll Area ---

/** @type {Map<string, { controller: AbortController, ro: ResizeObserver }>} */
const scrollAreaMap = new Map();

/**
 * Wires a custom scrollbar to a scroll-area: sizes/positions the thumb to reflect
 * scroll progress, hides the bar when content doesn't overflow, and lets the user
 * drag the thumb. The native scrollbar is hidden via CSS (.cn-scroll-area-viewport).
 * @param {HTMLElement} root - the [data-slot="scroll-area"] element
 * @param {string} id - unique ID for cleanup
 */
export function initScrollArea(root, id) {
    if (!root) return;
    destroyScrollArea(id);

    const viewport = root.querySelector('[data-slot="scroll-area-viewport"]');
    if (!viewport) return;
    const bars = [...root.querySelectorAll('[data-slot="scroll-area-scrollbar"]')];
    if (bars.length === 0) return;

    const controller = new AbortController();

    const update = () => {
        for (const bar of bars) {
            const vertical = bar.getAttribute('data-orientation') !== 'horizontal';
            const thumb = bar.querySelector('[data-slot="scroll-area-thumb"]');
            if (!thumb) continue;

            const contentSize = vertical ? viewport.scrollHeight : viewport.scrollWidth;
            const viewSize = vertical ? viewport.clientHeight : viewport.clientWidth;
            const overflow = contentSize - viewSize;

            // Hide the bar entirely when there's nothing to scroll.
            if (overflow <= 1) {
                bar.style.display = 'none';
                continue;
            }
            bar.style.display = '';

            const trackSize = vertical ? bar.clientHeight : bar.clientWidth;
            const thumbSize = Math.max((viewSize / contentSize) * trackSize, 20);
            const scrollPos = vertical ? viewport.scrollTop : viewport.scrollLeft;
            const maxThumbOffset = trackSize - thumbSize;
            const offset = overflow > 0 ? (scrollPos / overflow) * maxThumbOffset : 0;

            thumb.style.position = 'absolute';
            if (vertical) {
                thumb.style.insetInline = '0';
                thumb.style.top = `${offset}px`;
                thumb.style.height = `${thumbSize}px`;
            } else {
                thumb.style.insetBlock = '0';
                thumb.style.left = `${offset}px`;
                thumb.style.width = `${thumbSize}px`;
            }
        }
    };

    // Drag-to-scroll on each thumb.
    for (const bar of bars) {
        const vertical = bar.getAttribute('data-orientation') !== 'horizontal';
        const thumb = bar.querySelector('[data-slot="scroll-area-thumb"]');
        if (!thumb) continue;

        thumb.addEventListener('pointerdown', (e) => {
            e.preventDefault();
            e.stopPropagation();
            thumb.setPointerCapture(e.pointerId);

            const startPos = vertical ? e.clientY : e.clientX;
            const startScroll = vertical ? viewport.scrollTop : viewport.scrollLeft;
            const trackSize = vertical ? bar.clientHeight : bar.clientWidth;
            const contentSize = vertical ? viewport.scrollHeight : viewport.scrollWidth;
            const viewSize = vertical ? viewport.clientHeight : viewport.clientWidth;
            const thumbSize = Math.max((viewSize / contentSize) * trackSize, 20);
            const maxThumbOffset = trackSize - thumbSize;
            const overflow = contentSize - viewSize;

            const onMove = (ev) => {
                const delta = (vertical ? ev.clientY : ev.clientX) - startPos;
                const scrollDelta = maxThumbOffset > 0 ? (delta / maxThumbOffset) * overflow : 0;
                if (vertical) viewport.scrollTop = startScroll + scrollDelta;
                else viewport.scrollLeft = startScroll + scrollDelta;
            };
            const onUp = (ev) => {
                thumb.releasePointerCapture(ev.pointerId);
                thumb.removeEventListener('pointermove', onMove);
                thumb.removeEventListener('pointerup', onUp);
            };
            thumb.addEventListener('pointermove', onMove);
            thumb.addEventListener('pointerup', onUp);
        }, { signal: controller.signal });
    }

    viewport.addEventListener('scroll', update, { signal: controller.signal, passive: true });

    // Recompute when the viewport or its content resizes.
    const ro = new ResizeObserver(update);
    ro.observe(viewport);
    if (viewport.firstElementChild) ro.observe(viewport.firstElementChild);

    update();
    scrollAreaMap.set(id, { controller, ro });
}

/**
 * Tears down a scroll-area's listeners and observers.
 * @param {string} id
 */
export function destroyScrollArea(id) {
    const entry = scrollAreaMap.get(id);
    if (entry) {
        entry.controller.abort();
        entry.ro.disconnect();
        scrollAreaMap.delete(id);
    }
}

// --- Resizable Panels ---

/** @type {Map<string, AbortController>} */
const resizableMap = new Map();

/**
 * Wires pointer-drag resizing to a resizable-panel-group. On handle drag, the panel
 * immediately before and after the handle have their flex-grow adjusted so the total
 * is preserved (the rest of the group stays fixed). Direction is read from the group's
 * data-direction attribute (horizontal => width drag, vertical => height drag).
 * @param {HTMLElement} group - the [data-slot="resizable-panel-group"] element
 * @param {string} id - unique ID for cleanup
 */
export function initResizable(group, id) {
    if (!group) return;
    destroyResizable(id);

    const controller = new AbortController();
    resizableMap.set(id, controller);

    const vertical = group.getAttribute('data-direction') === 'vertical';
    // Only direct-child handles belong to THIS group (nested groups manage their own).
    const handles = [...group.children].filter(
        (c) => c.getAttribute && c.getAttribute('data-slot') === 'resizable-handle');

    for (const handle of handles) {
        handle.addEventListener('pointerdown', (e) => {
            const prev = handle.previousElementSibling;
            const next = handle.nextElementSibling;
            if (!prev || !next) return;

            e.preventDefault();
            handle.setPointerCapture(e.pointerId);
            handle.setAttribute('data-resize-handle-active', '');

            const startPos = vertical ? e.clientY : e.clientX;
            const prevRect = prev.getBoundingClientRect();
            const nextRect = next.getBoundingClientRect();
            const prevSize = vertical ? prevRect.height : prevRect.width;
            const nextSize = vertical ? nextRect.height : nextRect.width;
            const totalSize = prevSize + nextSize;

            // Preserve the combined flex-grow across the two panels so siblings don't shift.
            const prevGrow = parseFloat(getComputedStyle(prev).flexGrow) || 1;
            const nextGrow = parseFloat(getComputedStyle(next).flexGrow) || 1;
            const totalGrow = prevGrow + nextGrow;

            const onMove = (ev) => {
                const delta = (vertical ? ev.clientY : ev.clientX) - startPos;
                let newPrev = prevSize + delta;
                newPrev = Math.max(0, Math.min(newPrev, totalSize));
                const prevRatio = totalSize > 0 ? newPrev / totalSize : 0.5;
                prev.style.flexGrow = `${totalGrow * prevRatio}`;
                next.style.flexGrow = `${totalGrow * (1 - prevRatio)}`;
                prev.style.flexBasis = '0%';
                next.style.flexBasis = '0%';
            };
            const onUp = (ev) => {
                handle.releasePointerCapture(ev.pointerId);
                handle.removeAttribute('data-resize-handle-active');
                handle.removeEventListener('pointermove', onMove);
                handle.removeEventListener('pointerup', onUp);
                handle.removeEventListener('pointercancel', onUp);
            };
            handle.addEventListener('pointermove', onMove);
            handle.addEventListener('pointerup', onUp);
            handle.addEventListener('pointercancel', onUp);
        }, { signal: controller.signal });
    }
}

/**
 * Tears down resizable drag listeners for a given ID.
 * @param {string} id
 */
export function destroyResizable(id) {
    const controller = resizableMap.get(id);
    if (controller) {
        controller.abort();
        resizableMap.delete(id);
    }
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
