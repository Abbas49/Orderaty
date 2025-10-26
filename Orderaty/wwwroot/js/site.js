// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// QuickDeliver Website JavaScript
document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize all functionality
    initThemeSwitcher();
    initSearchFunctionality();
    initNavigation();
    initStoreCards();
    initCategoryItems();
    initResponsiveBehavior();
    initMobileMenu();
    
    // Theme Switcher Functionality
    function initThemeSwitcher() {
        const themeToggle = document.getElementById('theme-toggle');
        const themeDropdown = document.getElementById('theme-dropdown');
        const themeOptions = document.querySelectorAll('.theme-option');
        
        // Load saved theme or use default
        const savedTheme = localStorage.getItem('orderaty-theme') || 'green-fresh';
        applyTheme(savedTheme);
        
        // Toggle dropdown
        if (themeToggle) {
            themeToggle.addEventListener('click', function(e) {
                e.stopPropagation();
                themeDropdown.classList.toggle('show');
            });
        }
        
        // Close dropdown when clicking outside
        document.addEventListener('click', function(e) {
            if (themeDropdown && !themeDropdown.contains(e.target) && e.target !== themeToggle) {
                themeDropdown.classList.remove('show');
            }
        });
        
        // Handle theme selection
        themeOptions.forEach(option => {
            option.addEventListener('click', function() {
                const selectedTheme = this.getAttribute('data-theme');
                applyTheme(selectedTheme);
                localStorage.setItem('orderaty-theme', selectedTheme);
                
                // Update active state
                themeOptions.forEach(opt => opt.classList.remove('active'));
                this.classList.add('active');
                
                // Close dropdown
                themeDropdown.classList.remove('show');
                
                // Show notification
                showNotification(`Theme changed to ${this.querySelector('.theme-name').textContent}`, 'success');
            });
        });
        
        function applyTheme(themeName) {
            // Remove all theme attributes
            document.documentElement.removeAttribute('data-theme');
            
            // Apply selected theme
            if (themeName !== 'green-fresh') {
                document.documentElement.setAttribute('data-theme', themeName);
            }
            
            // Update active state in dropdown
            themeOptions.forEach(option => {
                if (option.getAttribute('data-theme') === themeName) {
                    option.classList.add('active');
                } else {
                    option.classList.remove('active');
                }
            });
        }
    }
    
    // Search functionality
    function initSearchFunctionality() {
        const searchInputs = document.querySelectorAll('.search-input input');
        const searchBtn = document.querySelector('.search-btn');
        
        // Handle search button click
        if (searchBtn) {
            searchBtn.addEventListener('click', function(e) {
                e.preventDefault();
                performSearch();
            });
        }
        
        // Handle Enter key in search inputs
        searchInputs.forEach(input => {
            input.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    performSearch();
                }
            });
            
            // Add focus/blur effects
            input.addEventListener('focus', function() {
                this.parentElement.style.boxShadow = '0 6px 20px rgba(3, 10, 18, 0.1)';
            });
            
            input.addEventListener('blur', function() {
                this.parentElement.style.boxShadow = '0 4px 12px rgba(3, 10, 18, 0.06)';
            });
        });
    }
    
    // Perform search functionality
    function performSearch() {
        const addressInput = document.querySelector('.search-input:first-child input');
        const searchInput = document.querySelector('.search-input:last-child input');
        
        const address = addressInput ? addressInput.value.trim() : '';
        const searchTerm = searchInput ? searchInput.value.trim() : '';
        
        if (!address && !searchTerm) {
            showNotification('Please enter an address or search term', 'warning');
            return;
        }
        
        // Simulate search loading
        const searchBtn = document.querySelector('.search-btn');
        if (searchBtn) {
            searchBtn.textContent = 'Searching...';
            searchBtn.disabled = true;
        }
        
        // Simulate API call
        setTimeout(() => {
            showNotification(`Searching for "${searchTerm}" near "${address}"...`, 'success');
            
            // Reset button
            if (searchBtn) {
                searchBtn.textContent = 'Search';
                searchBtn.disabled = false;
            }
        }, 1500);
    }
    
    // Navigation functionality
    function initNavigation() {
        const navLinks = document.querySelectorAll('.nav-link');
        const navbarItems = document.querySelectorAll('.navbar-item');
        
        // Handle old nav-link clicks
        navLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                // Allow links with actual URLs to navigate normally
                const href = this.getAttribute('href');
                if (href && href !== '#' && !href.startsWith('javascript:')) {
                    // Let the link navigate normally
                    return;
                }
                
                e.preventDefault();
                
                // Remove active class from all links
                navLinks.forEach(l => l.classList.remove('active'));
                
                // Add active class to clicked link
                this.classList.add('active');
                
                // Handle special navigation
                const linkText = this.textContent.trim();
                
                switch(linkText) {
                    case 'Home':
                        scrollToTop();
                        break;
                    case 'Browse':
                        scrollToSection('.browse-categories');
                        break;
                    case 'Cart':
                        showNotification('Cart functionality coming soon!', 'info');
                        break;
                    case 'Orders':
                        showNotification('Orders page coming soon!', 'info');
                        break;
                    case 'Profile':
                        showNotification('Profile page coming soon!', 'info');
                        break;
                }
            });
        });
        
        // Handle new navbar-item clicks
        navbarItems.forEach(item => {
            item.addEventListener('click', function(e) {
                // Allow links with actual URLs to navigate normally
                const href = this.getAttribute('href');
                if (href && href !== '#' && !href.startsWith('javascript:')) {
                    // Let the link navigate normally
                    return;
                }
                
                e.preventDefault();
                
                // Remove active class from all items
                navbarItems.forEach(i => i.classList.remove('active'));
                
                // Add active class to clicked item
                this.classList.add('active');
            });
        });
        
        // Set active state based on current page
        const currentPath = window.location.pathname;
        navbarItems.forEach(item => {
            const href = item.getAttribute('href');
            if (href === currentPath || (currentPath === '/' && href === '/')) {
                item.classList.add('active');
            }
        });
    }
    
    // Store cards functionality
    function initStoreCards() {
        const storeCards = document.querySelectorAll('.store-card');
        
        storeCards.forEach(card => {
            card.addEventListener('click', function() {
                const storeTitle = this.querySelector('.store-title').textContent;
                showNotification(`Opening ${storeTitle}...`, 'success');
                
                // Add click animation
                this.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 150);
            });
        });
    }
    
    // Category items functionality
    function initCategoryItems() {
        const categoryItems = document.querySelectorAll('.category-item');
        
        categoryItems.forEach(item => {
            item.addEventListener('click', function() {
                const categoryLabel = this.querySelector('.category-label').textContent;
                showNotification(`Browsing ${categoryLabel}...`, 'success');
                
                // Add click animation
                this.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 150);
            });
        });
    }
    
    // Responsive behavior
    function initResponsiveBehavior() {
        // Handle navbar scroll effect
        const navbar = document.querySelector('.navbar');
        const header = document.querySelector('.header');
        
        let lastScrollY = window.scrollY;
        
        window.addEventListener('scroll', function() {
            const currentScrollY = window.scrollY;
            
            // Handle new navbar
            if (navbar) {
                if (currentScrollY > 50) {
                    navbar.style.boxShadow = '0 4px 20px var(--shadow-md)';
                    navbar.style.backdropFilter = 'blur(12px)';
                } else {
                    navbar.style.boxShadow = '0 2px 10px var(--shadow-sm)';
                    navbar.style.backdropFilter = 'blur(10px)';
                }
            }
            
            // Handle old header if it exists
            if (header) {
                if (currentScrollY > 100) {
                    header.style.background = 'var(--bg-white)';
                    header.style.backdropFilter = 'blur(10px)';
                    header.style.boxShadow = '0 2px 8px var(--shadow-md)';
                } else {
                    header.style.background = 'var(--bg-white)';
                    header.style.backdropFilter = 'none';
                    header.style.boxShadow = '0 1px 3px var(--shadow-sm)';
                }
            }
            
            lastScrollY = currentScrollY;
        });
        
        // Handle window resize
        window.addEventListener('resize', function() {
            // Recalculate layouts if needed
            adjustLayoutForScreenSize();
        });
    }
    
    // Adjust layout for different screen sizes
    function adjustLayoutForScreenSize() {
        const width = window.innerWidth;
        
        if (width <= 768) {
            // Mobile optimizations
            document.body.classList.add('mobile');
        } else {
            document.body.classList.remove('mobile');
        }
    }
    
    // Utility functions
    function scrollToTop() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
    
    function scrollToSection(selector) {
        const section = document.querySelector(selector);
        if (section) {
            section.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    }
    
    function showNotification(message, type = 'info') {
        // Remove existing notifications
        const existingNotification = document.querySelector('.notification');
        if (existingNotification) {
            existingNotification.remove();
        }
        
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close">&times;</button>
            </div>
        `;
        
        // Add styles
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#10b981' : type === 'warning' ? '#f59e0b' : type === 'error' ? '#ef4444' : '#3b82f6'};
            color: white;
            padding: 12px 16px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            z-index: 1000;
            max-width: 300px;
            transform: translateX(100%);
            transition: transform 0.3s ease;
        `;
        
        // Add to page
        document.body.appendChild(notification);
        
        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);
        
        // Auto remove after 4 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.style.transform = 'translateX(100%)';
                setTimeout(() => {
                    if (notification.parentNode) {
                        notification.remove();
                    }
                }, 300);
            }
        }, 4000);
        
        // Close button functionality
        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.addEventListener('click', () => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 300);
        });
    }
    
    // Initialize layout on load
    adjustLayoutForScreenSize();
    
    // Add intersection observer for animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);
    
    // Observe elements for animation
    const animatedElements = document.querySelectorAll('.store-card, .category-item, .section-title');
    animatedElements.forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });
    
    // Mobile menu functionality
    function initMobileMenu() {
        const navbarToggle = document.getElementById('navbar-toggle');
        const navbarMenu = document.getElementById('navbar-menu');
        const navbarItems = document.querySelectorAll('.navbar-item');
        
        if (navbarToggle && navbarMenu) {
            // Toggle menu
            navbarToggle.addEventListener('click', function(e) {
                e.stopPropagation();
                navbarMenu.classList.toggle('active');
                navbarToggle.classList.toggle('active');
                
                // Prevent body scroll when menu is open
                if (navbarMenu.classList.contains('active')) {
                    document.body.style.overflow = 'hidden';
                } else {
                    document.body.style.overflow = '';
                }
            });
            
            // Close menu when clicking on nav items
            navbarItems.forEach(item => {
                item.addEventListener('click', function() {
                    if (window.innerWidth <= 768) {
                        navbarMenu.classList.remove('active');
                        navbarToggle.classList.remove('active');
                        document.body.style.overflow = '';
                    }
                });
            });
            
            // Close menu when clicking outside
            document.addEventListener('click', function(e) {
                if (navbarMenu.classList.contains('active') && 
                    !navbarMenu.contains(e.target) && 
                    !navbarToggle.contains(e.target)) {
                    navbarMenu.classList.remove('active');
                    navbarToggle.classList.remove('active');
                    document.body.style.overflow = '';
                }
            });
            
            // Close menu on window resize to desktop
            window.addEventListener('resize', function() {
                if (window.innerWidth > 768 && navbarMenu.classList.contains('active')) {
                    navbarMenu.classList.remove('active');
                    navbarToggle.classList.remove('active');
                    document.body.style.overflow = '';
                }
            });
        }
        
        // Handle old mobile menu button if it exists
        const mobileMenuBtn = document.getElementById('mobile-menu-btn');
        const navMenu = document.getElementById('nav-menu');
        
        if (mobileMenuBtn && navMenu) {
            mobileMenuBtn.addEventListener('click', function() {
                navMenu.classList.toggle('active');
                mobileMenuBtn.classList.toggle('active');
                
                // Prevent body scroll when menu is open
                if (navMenu.classList.contains('active')) {
                    document.body.style.overflow = 'hidden';
                } else {
                    document.body.style.overflow = '';
                }
            });
            
            // Close menu when clicking on nav links
            const navLinks = navMenu.querySelectorAll('.nav-link');
            navLinks.forEach(link => {
                link.addEventListener('click', function() {
                    navMenu.classList.remove('active');
                    mobileMenuBtn.classList.remove('active');
                    document.body.style.overflow = '';
                });
            });
            
            // Close menu when clicking outside
            document.addEventListener('click', function(e) {
                if (!navMenu.contains(e.target) && !mobileMenuBtn.contains(e.target)) {
                    navMenu.classList.remove('active');
                    mobileMenuBtn.classList.remove('active');
                    document.body.style.overflow = '';
                }
            });
            
            // Close menu on window resize to desktop size
            window.addEventListener('resize', function() {
                if (window.innerWidth > 768) {
                    navMenu.classList.remove('active');
                    mobileMenuBtn.classList.remove('active');
                    document.body.style.overflow = '';
                }
            });
        }
    }
    
    console.log('QuickDeliver website initialized successfully!');
});
