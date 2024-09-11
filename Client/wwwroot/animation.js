function showAnimationForTwoSeconds() {
    var animation = document.getElementById('

Animation');
    animation.style.display = 'block';
    setTimeout(() => {
        animation.style.display = 'none';
    }, 10000); // 2 seconds
}