
var onMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);

function isInViewport(element) {
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}

var contentVideos = document.getElementsByClassName("content-video");
var backgroundVideo = document.getElementById("background-video");


function checkVideos() {
    for (var i = 0; i < contentVideos.length; i++) {
        var video = contentVideos[i];
        if (isInViewport(video)) {
            video.play();
        }
        else {
            video.pause();
        }
    }

    if (window.scrollY >= window.innerHeight) {
        backgroundVideo.pause();
    }
    else {
        backgroundVideo.play();
    }
}

if (!onMobile) {
    window.addEventListener("scroll", () => {
        checkVideos();
    });

    window.addEventListener("resize", () => {
        checkVideos();
    });

    checkVideos();

    for (var i = 0; i < contentVideos.length; i++) {
        var video = contentVideos[i];
        video.autoplay = true;
        video.controls = false;
    }

    backgroundVideo.autoplay = true;

}
else {
    for (var i = 0; i < contentVideos.length; i++) {
        var video = contentVideos[i];
        video.autoplay = false;
        video.controls = true;
    }

    backgroundVideo.autoplay = false;
}