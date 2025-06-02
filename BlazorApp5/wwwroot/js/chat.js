//window.scrollToBottom = (element) => {
//    if (element) {
//        element.scrollTop = element.scrollHeight;
//    }
//};

window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTo({
            top: element.scrollHeight,
            behavior: "smooth"
        });
    }
};