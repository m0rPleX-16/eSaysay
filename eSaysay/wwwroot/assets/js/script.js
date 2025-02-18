document.addEventListener('DOMContentLoaded', function () {
    const toggler = document.querySelector('.sidebar-toggler');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');

    toggler.addEventListener('click', function () {
        sidebar.classList.toggle('collapsed');
        mainContent.classList.toggle('collapsed');
    });

    const carousels = document.querySelectorAll('.carousel');
    carousels.forEach(carousel => {
        const cardsContainer = carousel.querySelector('.lesson-cards');
        const cards = carousel.querySelectorAll('.lesson-card');
        const prevButton = carousel.querySelector('.carousel-control.prev');
        const nextButton = carousel.querySelector('.carousel-control.next');

        let currentIndex = 0;

        function updateCarousel() {
            const cardWidth = cards[0].offsetWidth + 20; // Card width + gap
            cardsContainer.style.transform = `translateX(${-currentIndex * cardWidth}px)`;
        }

        prevButton.addEventListener('click', () => {
            if (currentIndex > 0) {
                currentIndex--;
                updateCarousel();
            }
        });

        nextButton.addEventListener('click', () => {
            if (currentIndex < cards.length - 1) {
                currentIndex++;
                updateCarousel();
            }
        });
    });
});