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
    // No focusable children: focus the container itself (modal content divs carry
    // tabindex="-1") so focus still enters the dialog — its Escape handler can then
    // fire — and the Tab handler below pins focus in place (APG modal dialog).
    if (focusable.length > 0) focusable[0].focus();
    else element.focus();

    element.addEventListener('keydown', (e) => {
        if (e.key !== 'Tab') return;

        const currentFocusable = getFocusableElements(element);
        if (currentFocusable.length === 0) {
            e.preventDefault();
            return;
        }
        const first = currentFocusable[0];
        const last = currentFocusable[currentFocusable.length - 1];
        // Focus can sit on a non-tabbable element (e.g. the container itself after
        // a click on dialog text) — treat that as outside the list and wrap.
        const inList = currentFocusable.includes(document.activeElement);

        if (e.shiftKey) {
            if (!inList || document.activeElement === first) {
                e.preventDefault();
                last.focus();
            }
        } else {
            if (!inList || document.activeElement === last) {
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
 * @param {HTMLElement} [excluded] - optional element (typically the trigger) treated
 *   as INSIDE the dismissable layer: pointerdown on it does not fire the callback,
 *   so the trigger's own click toggle performs the close instead of a
 *   close-then-reopen race (Radix DismissableLayer behavior)
 */
export function onOutsideClick(element, id, dotnetRef, methodName, excluded) {
    if (!element) return;
    cleanup(id);

    const controller = new AbortController();
    cleanupMap.set(id, controller);

    setTimeout(() => {
        if (controller.signal.aborted) return;
        document.addEventListener('pointerdown', (e) => {
            if (!element.isConnected) return;
            if (excluded && (excluded === e.target || excluded.contains(e.target))) return;
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
        // Reserve the scrollbar gap BEFORE position:fixed removes the scrollbar,
        // otherwise the page reflows ~15px wider on scrollbar-visible platforms
        // (react-remove-scroll does the same). Overlay scrollbars yield gap 0.
        const gap = window.innerWidth - document.documentElement.clientWidth;
        if (gap > 0) document.body.style.paddingRight = `${gap}px`;
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
    document.body.style.paddingRight = '';
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

    // RTL awareness (Radix/Floating UI semantics): align start/end are LOGICAL on the
    // inline axis — under dir=rtl, align=start hugs the trigger's RIGHT edge. Side
    // stays physical, except submenus (flipSideOnRtl) which open toward the reading
    // direction. Direction is read from the reference so per-subtree dir works.
    const rtl = getComputedStyle(reference).direction === 'rtl';
    if (rtl && options.flipSideOnRtl && (side === 'left' || side === 'right')) {
        side = side === 'left' ? 'right' : 'left';
    }
    const alignH = rtl && align !== 'center'
        ? (align === 'start' ? 'end' : 'start')
        : align;

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
                left = calcAlignHorizontal(refRect, floatRect, alignH, alignOffset);
                break;
            case 'bottom':
                top = refRect.bottom + sideOffset;
                left = calcAlignHorizontal(refRect, floatRect, alignH, alignOffset);
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

    // Shift along the alignment (cross) axis — Floating UI's `shift` middleware
    // analog. Flipping only swaps the main-axis side, so a popup anchored near a
    // viewport edge (or wider than the space beside its trigger) must be slid back
    // into view. Applied BEFORE viewportPos is captured so the arrow keeps pointing
    // at the trigger. Math.max runs last so content wider than the viewport pins to
    // the pad edge instead of hanging off the left/top.
    if (actualSide === 'top' || actualSide === 'bottom') {
        pos.left = Math.max(VIEWPORT_PAD, Math.min(pos.left, viewportW - floatRect.width - VIEWPORT_PAD));
    } else {
        pos.top = Math.max(VIEWPORT_PAD, Math.min(pos.top, viewportH - floatRect.height - VIEWPORT_PAD));
    }

    // Anchor the open/close zoom animation to the trigger edge, mirroring
    // Base UI's --transform-origin (read via `origin-(--transform-origin)`).
    let originX, originY;
    if (actualSide === 'top' || actualSide === 'bottom') {
        originY = actualSide === 'top' ? 'bottom' : 'top';
        originX = alignH === 'start' ? 'left' : alignH === 'end' ? 'right' : 'center';
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
 * @param {object} options - { selector: string, orientation: 'vertical'|'horizontal'|'both',
 *   autoFocus: boolean, initialSelector: string }
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

    // Typeahead state (APG: printable characters move focus to the matching item).
    let typeaheadBuffer = '';
    let typeaheadTimer = 0;

    container.addEventListener('keydown', (e) => {
        // A nested submenu's own listener (attached to a descendant container)
        // already consumed this key — without this guard the parent menu would
        // process it a second time and focus would jump two items per press.
        if (e.defaultPrevented) return;

        const items = getNavigableItems(container, selector);
        if (items.length === 0) return;

        const currentIndex = items.indexOf(document.activeElement);

        // A horizontal nav (menubar root, tabs list) must not act on keys that bubble
        // up while focus is elsewhere — e.g. inside an open menubar menu. Without this,
        // ArrowRight from a menu item teleported focus to the FIRST trigger (indexOf
        // returned -1) while the menu stayed open.
        if (orientation === 'horizontal' && currentIndex === -1) return;

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
        } else if (e.key === 'ArrowRight' && orientation === 'vertical') {
            // APG menu: ArrowRight on a submenu trigger opens it — the opening
            // sub-content's own keyboard setup then focuses its first item.
            const active = document.activeElement;
            if (active && active.matches('[data-slot$="sub-trigger"]') && !isItemDisabled(active)) {
                e.preventDefault();
                e.stopPropagation();
                active.click();
            }
        } else if (e.key === 'ArrowLeft' && orientation === 'vertical') {
            // APG menu: ArrowLeft inside a submenu closes it and returns focus to
            // the parent trigger (the kbd-cleanup focus restore handles the return).
            if ((container.getAttribute('data-slot') ?? '').endsWith('sub-content')) {
                e.preventDefault();
                e.stopPropagation();
                if (dotnetRef && escapeMethodName) {
                    dotnetRef.invokeMethodAsync(escapeMethodName);
                }
            }
        } else if (e.key === 'Escape') {
            e.preventDefault();
            // Only the innermost open surface may close (APG): without
            // stopPropagation the key bubbles to ancestor menu listeners and to
            // Blazor's document-level delegation (e.g. a Dialog's @onkeydown),
            // closing every layer at once.
            e.stopPropagation();
            if (dotnetRef && escapeMethodName) {
                dotnetRef.invokeMethodAsync(escapeMethodName);
            }
        } else if (e.key === 'Enter' || e.key === ' ') {
            if (currentIndex >= 0 && !isItemDisabled(items[currentIndex])) {
                e.preventDefault();
                items[currentIndex].click();
            }
        } else if (e.key === 'Tab') {
            // APG menu: Tab moves focus out of the menu and closes it. No
            // preventDefault — the browser moves focus naturally, and the
            // kbd-cleanup focus-restore guard skips restoration because focus
            // lands outside the container. Persistent widgets (autoFocus=false)
            // keep default Tab behavior.
            if (autoFocus && dotnetRef && escapeMethodName) {
                dotnetRef.invokeMethodAsync(escapeMethodName);
            }
        } else if (e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey) {
            // Typeahead (APG): focus the next enabled item whose text starts with
            // the typed string. Skipped when an input hosts the keystrokes
            // (Combobox/Command search fields must keep filtering).
            if ((e.target.matches && e.target.matches('input, textarea, [contenteditable]'))
                || container.querySelector('input') !== null) return;
            typeaheadBuffer += e.key.toLowerCase();
            clearTimeout(typeaheadTimer);
            typeaheadTimer = setTimeout(() => { typeaheadBuffer = ''; }, 1000);
            // Start after the current item, wrap, and check the current item last
            // so a multi-char buffer can still match it.
            const start = currentIndex;
            for (let i = 1; i <= items.length; i++) {
                const candidate = items[(start + i) % items.length];
                if (!isItemDisabled(candidate)
                    && (candidate.textContent ?? '').trim().toLowerCase().startsWith(typeaheadBuffer)) {
                    candidate.focus();
                    break;
                }
            }
        }
    }, { signal: controller.signal });

    // Focus the first match of initialSelector (e.g. the selected option, or a
    // combobox's search input), else the first enabled item (menus only —
    // persistent widgets keep focus).
    if (autoFocus) {
        const preferred = options?.initialSelector
            ? container.querySelector(options.initialSelector) : null;
        if (preferred && !isItemDisabled(preferred)) {
            preferred.focus();
        } else {
            const items = getNavigableItems(container, selector);
            const firstEnabled = findNextEnabled(items, -1);
            if (firstEnabled >= 0) items[firstEnabled].focus();
        }
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
 * Items inside a NESTED submenu are excluded — the submenu container runs its own
 * navigation setup, and without the filter Home/End (and wrap-around) from the
 * parent menu would jump into submenu items.
 * @param {HTMLElement} container
 * @param {string} selector
 * @returns {HTMLElement[]}
 */
function getNavigableItems(container, selector) {
    return [...container.querySelectorAll(selector)].filter((el) => {
        const sub = el.closest('[data-slot$="sub-content"]');
        return !sub || sub === container || !container.contains(sub);
    });
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
                thumb.removeEventListener('pointercancel', onUp);
            };
            thumb.addEventListener('pointermove', onMove);
            thumb.addEventListener('pointerup', onUp);
            thumb.addEventListener('pointercancel', onUp);
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
 * Splits the combined flex-grow of the two panels flanking a handle so the previous
 * panel takes `newPrev` of `totalSize` pixels. Shared by pointer-drag and keyboard
 * resizing. Returns the resulting prev ratio (0..1) for aria-valuenow updates.
 * @param {HTMLElement} prev
 * @param {HTMLElement} next
 * @param {number} newPrev - desired pixel size of the previous panel
 * @param {number} totalSize - combined pixel size of both panels
 * @param {number} totalGrow - combined flex-grow of both panels
 * @returns {number}
 */
function applyPanelSizes(prev, next, newPrev, totalSize, totalGrow) {
    newPrev = Math.max(0, Math.min(newPrev, totalSize));
    const prevRatio = totalSize > 0 ? newPrev / totalSize : 0.5;
    prev.style.flexGrow = `${totalGrow * prevRatio}`;
    next.style.flexGrow = `${totalGrow * (1 - prevRatio)}`;
    prev.style.flexBasis = '0%';
    next.style.flexBasis = '0%';
    return prevRatio;
}

/**
 * Wires pointer-drag AND keyboard resizing to a resizable-panel-group. On handle
 * drag (or Arrow/Home/End on the focused handle), the panel immediately before and
 * after the handle have their flex-grow adjusted so the total is preserved (the rest
 * of the group stays fixed). Direction is read from the group's data-direction
 * attribute (horizontal => width drag, vertical => height drag). Also maintains
 * aria-valuenow/aria-controls on each role="separator" handle (APG window splitter).
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

    // Measures the panels flanking a handle. Recomputed per interaction — panel
    // sizes change between events (other handles, container resizes).
    function measurePanels(handle) {
        const prev = handle.previousElementSibling;
        const next = handle.nextElementSibling;
        if (!prev || !next) return null;
        const prevRect = prev.getBoundingClientRect();
        const nextRect = next.getBoundingClientRect();
        const prevSize = vertical ? prevRect.height : prevRect.width;
        const nextSize = vertical ? nextRect.height : nextRect.width;
        // Preserve the combined flex-grow across the two panels so siblings don't shift.
        const prevGrow = parseFloat(getComputedStyle(prev).flexGrow) || 1;
        const nextGrow = parseFloat(getComputedStyle(next).flexGrow) || 1;
        return { prev, next, prevSize, totalSize: prevSize + nextSize, totalGrow: prevGrow + nextGrow };
    }

    handles.forEach((handle, index) => {
        // APG window splitter: expose the split position to assistive tech.
        // aria-valuemin/valuemax are rendered statically by ResizableHandleCn.
        const initial = measurePanels(handle);
        if (initial) {
            if (!initial.prev.id) initial.prev.id = `${id}-panel-${index}`;
            handle.setAttribute('aria-controls', initial.prev.id);
            handle.setAttribute('aria-valuenow',
                `${Math.round((initial.totalSize > 0 ? initial.prevSize / initial.totalSize : 0.5) * 100)}`);
        }

        handle.addEventListener('pointerdown', (e) => {
            const panels = measurePanels(handle);
            if (!panels) return;
            const { prev, next, prevSize, totalSize, totalGrow } = panels;

            e.preventDefault();
            handle.setPointerCapture(e.pointerId);
            handle.setAttribute('data-resize-handle-active', '');

            const startPos = vertical ? e.clientY : e.clientX;

            const onMove = (ev) => {
                const delta = (vertical ? ev.clientY : ev.clientX) - startPos;
                const prevRatio = applyPanelSizes(prev, next, prevSize + delta, totalSize, totalGrow);
                handle.setAttribute('aria-valuenow', `${Math.round(prevRatio * 100)}`);
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

        // APG window splitter keyboard support: arrows along the group axis move
        // the splitter (Shift = coarse step), Home/End snap to min/max. Panel
        // Min/MaxSize still clamp via the min/max CSS ResizablePanelCn emits.
        handle.addEventListener('keydown', (e) => {
            const prevKey = vertical ? 'ArrowUp' : 'ArrowLeft';
            const nextKey = vertical ? 'ArrowDown' : 'ArrowRight';
            if (e.key !== prevKey && e.key !== nextKey && e.key !== 'Home' && e.key !== 'End') return;

            const panels = measurePanels(handle);
            if (!panels) return;
            const { prev, next, prevSize, totalSize, totalGrow } = panels;

            e.preventDefault();
            const step = totalSize * (e.shiftKey ? 0.10 : 0.02);
            let newPrev;
            if (e.key === 'Home') newPrev = 0;
            else if (e.key === 'End') newPrev = totalSize;
            else if (e.key === prevKey) newPrev = prevSize - step;
            else newPrev = prevSize + step;

            const prevRatio = applyPanelSizes(prev, next, newPrev, totalSize, totalGrow);
            handle.setAttribute('aria-valuenow', `${Math.round(prevRatio * 100)}`);
        }, { signal: controller.signal });
    });
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

// --- Media query watcher & sidebar shortcut ---

let _mediaWatchCounter = 0;

/**
 * Watches a CSS media query and reports match-state changes to .NET. Invokes the
 * callback immediately with the current state, then on every change (used by
 * SidebarProviderCn to drive the mobile Sheet branch below 768px).
 * @param {string} query - media query, e.g. '(max-width: 767px)'
 * @param {object} dotnetRef - .NET object reference for callback
 * @param {string} methodName - [JSInvokable] method receiving a bool
 * @returns {string} watcher ID for unwatchMedia
 */
export function watchMedia(query, dotnetRef, methodName) {
    const id = `media-watch-${++_mediaWatchCounter}`;
    const mql = window.matchMedia(query);
    const notify = () => dotnetRef.invokeMethodAsync(methodName, mql.matches);
    mql.addEventListener('change', notify);
    cleanupMap.set(id, { abort: () => mql.removeEventListener('change', notify) });
    notify();
    return id;
}

/**
 * Stops a media-query watcher created by watchMedia.
 * @param {string} id
 */
export function unwatchMedia(id) {
    cleanup(id);
}

/**
 * Registers the global Ctrl/Cmd+B sidebar-toggle shortcut (shadcn's
 * SIDEBAR_KEYBOARD_SHORTCUT). preventDefault must run synchronously in JS —
 * Blazor @onkeydown can't stop the browser's own Ctrl+B binding. Torn down
 * via cleanup(id).
 * @param {string} id - unique ID for cleanup
 * @param {object} dotnetRef - .NET object reference for callback
 * @param {string} methodName - [JSInvokable] method toggling the sidebar
 */
export function initSidebarShortcut(id, dotnetRef, methodName) {
    cleanup(id);
    const controller = new AbortController();
    cleanupMap.set(id, controller);
    window.addEventListener('keydown', (e) => {
        if (e.key === 'b' && (e.metaKey || e.ctrlKey)) {
            e.preventDefault();
            dotnetRef.invokeMethodAsync(methodName);
        }
    }, { signal: controller.signal });
}

// --- Utilities ---

/**
 * Gets all focusable elements within a container. Invisible elements are excluded:
 * calling .focus() on a hidden element is a silent no-op, which would break the
 * focus-trap wrap (preventDefault fires but focus never moves).
 * @param {HTMLElement} container
 * @returns {HTMLElement[]}
 */
function getFocusableElements(container) {
    return [...container.querySelectorAll(
        'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
    )].filter((el) => el.checkVisibility ? el.checkVisibility() : el.offsetParent !== null);
}

// --- Context menu positioning (self-contained, appended) ---

/**
 * Positions a context-menu popup at pointer coordinates with Radix-style
 * viewport collision handling: flips across the pointer when the menu would
 * overflow the right/bottom edge, clamps into the viewport with an 8px pad,
 * and sets --available-height / --transform-origin on the element so
 * `max-h-(--available-height)` and `origin-(--transform-origin)` resolve.
 * @param {HTMLElement} el
 * @param {number} x
 * @param {number} y
 */
export function positionContextMenu(el, x, y) {
    if (!el) return;
    const pad = 8;
    const width = el.offsetWidth;
    const height = el.offsetHeight;

    let flippedX = false;
    let flippedY = false;

    if (x + width > window.innerWidth - pad) {
        x = x - width;
        flippedX = true;
    }
    x = Math.max(pad, Math.min(x, window.innerWidth - width - pad));

    if (y + height > window.innerHeight - pad) {
        y = y - height;
        flippedY = true;
    }
    y = Math.max(pad, Math.min(y, window.innerHeight - height - pad));

    el.style.left = `${x}px`;
    el.style.top = `${y}px`;
    el.style.setProperty('--available-height', `${window.innerHeight - y - pad}px`);
    el.style.setProperty('--transform-origin', `${flippedY ? 'bottom' : 'top'} ${flippedX ? 'right' : 'left'}`);
}

// --- Command virtual highlight (self-contained, appended) ---

/**
 * Scrolls a command item into view inside its scrollable list. Used by the
 * cmdk-style virtual highlight in CommandCn: DOM focus stays in the input,
 * so the highlighted item must be scrolled into view manually.
 * @param {HTMLElement} listEl - the scrollable list (or root) container
 * @param {string} itemId - id of the highlighted item element
 */
export function scrollItemIntoView(listEl, itemId) {
    if (!listEl || !itemId) return;
    const item = listEl.querySelector('#' + CSS.escape(itemId)) ?? document.getElementById(itemId);
    if (item) item.scrollIntoView({ block: 'nearest' });
}

/**
 * Force-syncs a DOM input's value property. Blazor only patches `value` when the
 * rendered attribute CHANGES, so a handler that rejects input (leaving the bound
 * value unchanged) produces no diff and the rejected characters stay in the DOM.
 * @param {HTMLElement} element - the input element
 * @param {string} value - the value the DOM must show
 */
export function setInputValue(element, value) {
    if (!element) return;
    if (element.value !== value) element.value = value ?? '';
}

/**
 * preventDefault()s the listed keys on matching descendants, without handling them.
 * Blazor's @onkeydown cannot conditionally suppress the browser default (the
 * :preventDefault directive is unconditional and would break Tab/Enter), so C#
 * key handlers that move focus/selection with arrows still let the page scroll.
 * This guard swallows only the given keys, only when the event target matches.
 * @param {HTMLElement} container
 * @param {string} id - cleanup key (release via cleanup(id))
 * @param {string[]} keys - e.g. ['ArrowDown','ArrowUp','Home','End']
 * @param {string} selector - target filter, e.g. '[role="radio"]'
 */
export function preventKeyDefaults(container, id, keys, selector) {
    if (!container) return;
    const controller = new AbortController();
    container.addEventListener('keydown', (e) => {
        if (!keys.includes(e.key)) return;
        if (selector && !(e.target instanceof Element && e.target.matches(selector))) return;
        e.preventDefault();
    }, { signal: controller.signal });
    cleanupMap.set(id, controller);
}
