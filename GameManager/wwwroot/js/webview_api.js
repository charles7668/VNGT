window.resizeHandlers = {};

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

function disableHtmlOverflow(){
    window.scrollTo({
        top: 0,
        behavior: 'instant'
    })
    document.querySelector("html").classList.add("overflow-y-hidden");
}

function enableHtmlOverflow(){
    document.querySelector("html").classList.remove("overflow-y-hidden");
}