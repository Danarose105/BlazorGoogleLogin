function showAnimationForTwoSeconds() {
    var animation = document.getElementById('

Animation');
    animation.style.display = 'block';
    setTimeout(() => {
        animation.style.display = 'none';
    }, 4000); // 2 seconds
}