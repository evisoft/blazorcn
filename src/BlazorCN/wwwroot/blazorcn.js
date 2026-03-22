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
