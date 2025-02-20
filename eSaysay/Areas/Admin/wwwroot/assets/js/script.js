document.addEventListener('DOMContentLoaded', function () {
    const toggler = document.querySelector('.sidebar-toggler');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');
    const footer = document.querySelector('.footer');

    toggler.addEventListener('click', function () {
        sidebar.classList.toggle('collapsed');
        mainContent.classList.toggle('collapsed');
        footer.classList.toggle('collapsed');
    });

    const carousels = document.querySelectorAll('.carousel');
    carousels.forEach(carousel => {
        const cardsContainer = carousel.querySelector('.lesson-cards');
        const cards = carousel.querySelectorAll('.lesson-card');
        const prevButton = carousel.querySelector('.carousel-control.prev');
        const nextButton = carousel.querySelector('.carousel-control.next');

        let currentIndex = 0;

        function updateCarousel() {
            const cardWidth = cards[0].offsetWidth + 20;
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

document.addEventListener('DOMContentLoaded', function () {
    const navLinks = document.querySelectorAll('#navmenu ul li a');

    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            navLinks.forEach(link => link.classList.remove('active'));

            this.classList.add('active');
        });
    });
});