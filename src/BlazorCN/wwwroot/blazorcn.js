// BlazorCN JS Interop — minimal behaviors that CSS can't handle

/** @type {Map<string, AbortController|{abort: Function}>} */
const cleanupMap = new Map();

/**
 * Traps focus within an element (for modals/dialogs).
 * @param {HTMLElement} element
 * @param {string} id - unique ID for cleanup
 */
export function trapFocus(element, id) {
    if (!element) return;
    cleanup(id);

    const controller = new AbortController();
    cleanupMap.set(id, controller);

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
        document.addEventListener('pointerdown', (e) => {
            if (!element.contains(e.target)) {
                dotnetRef.invokeMethodAsync(methodName);
            }
        }, { signal: controller.signal });
    }, 0);
}

/**
 * Locks body scroll (for modals).
 * @param {string} id
 */
export function lockScroll(id) {
    cleanup(id);
    const scrollY = window.scrollY;
    document.body.style.position = 'fixed';
    document.body.style.top = `-${scrollY}px`;
    document.body.style.left = '0';
    document.body.style.right = '0';
    document.body.style.overflow = 'hidden';
    cleanupMap.set(id, { abort: () => unlockScrollInternal(scrollY) });
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
    const existing = cleanupMap.get(id);
    if (existing) {
        if (typeof existing.abort === 'function') existing.abort();
        cleanupMap.delete(id);
    }
}

// --- Floating Positioning ---

/** @type {Map<string, { reference: HTMLElement, floating: HTMLElement, options: object, update: Function }>} */
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

    floatingMap.set(id, { reference, floating, options: opts, update });
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
    const floatRect = floating.getBoundingClientRect();
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
