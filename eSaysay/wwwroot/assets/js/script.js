document.addEventListener('DOMContentLoaded', function () {
    const toggler = document.querySelector('.sidebar-toggler');
    const sidebar = document.querySelector('.sidebar');
    const menuToggler = document.querySelector('.menu-toggler');
    const mainContent = document.querySelector('.main-content');
    const footer = document.querySelector('.footer');

    const collapsedSidebarHeight = "56px";
    const fullSidebarHeight = "calc(100vh - 124px)";

    // Toggle sidebar's collapsed state
    toggler.addEventListener('click', function () {
        sidebar.classList.toggle('collapsed');
        mainContent.classList.toggle('collapsed');
        footer.classList.toggle('collapsed');
    });

    // Update sidebar height and menu toggle text
    const toggleMenu = (isMenuActive) => {
        sidebar.style.height = isMenuActive ? `${sidebar.scrollHeight}px` : collapsedSidebarHeight;
        menuToggler.querySelector("span").innerText = isMenuActive ? "close" : "menu";
    };

    // Toggle menu active class and adjust height
    menuToggler.addEventListener("click", () => {
        toggleMenu(sidebar.classList.toggle("menu-active"));
    });

    // Adjusting sidebar height on window resize
    window.addEventListener("resize", () => {
        if (window.innerWidth >= 1024) {
            sidebar.style.height = fullSidebarHeight;
        } else {
            sidebar.classList.remove("collapsed");
            sidebar.style.height = "auto";
            toggleMenu(sidebar.classList.contains("menu-active"));
        }
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