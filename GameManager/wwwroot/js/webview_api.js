window.resizeHandlers = {};
window.hotkeyHandlers = {};

let observer = null;

function debounce(func, timeout = 200) {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => { func.apply(this, args); }, timeout);
    };
}

window.resizeHandlers.addResizeListener = function (dotNetReference) {
    let resizeFunc = () => {
        dotNetReference.invokeMethodAsync('OnResizeEvent', window.innerWidth, window.innerHeight);
    }
    let div = document.getElementsByTagName('html')[0];
    observer = new ResizeObserver(debounce(resizeFunc));
    observer.observe(div);
};

window.resizeHandlers.removeResizeListener = function () {
    if (observer !== null) {
        let div = document.getElementsByTagName('html')[0];
        observer.unobserve(div);
    }
};

function getCardListWidth() {
    let cardListDiv = document.getElementById('card-list');
    return cardListDiv.clientWidth;
}

function remToPixels(rem) {
    return rem * parseFloat(getComputedStyle(document.documentElement).fontSize);
}

window.hotkeyHandlers.registerMainPageHotkey = (dotNetHelper) => {
    document.addEventListener('keydown', function (e) {
        if (e.ctrlKey && e.key === 'f') {
            e.preventDefault();
            dotNetHelper.invokeMethodAsync('OnSearchHotkeyTriggered');
        }
    });
};