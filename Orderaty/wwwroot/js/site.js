// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// QuickDeliver Website JavaScript
document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize all functionality
    initSearchFunctionality();
    initNavigation();
    initStoreCards();
    initCategoryItems();
    initResponsiveBehavior();
    initMobileMenu();
    
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
        // Handle mobile menu (if needed in future)
        const header = document.querySelector('.header');
        
        // Add scroll effect to header
        let lastScrollY = window.scrollY;
        
        window.addEventListener('scroll', function() {
            const currentScrollY = window.scrollY;
            
            if (currentScrollY > 100) {
                header.style.background = 'rgba(255, 255, 255, 0.95)';
                header.style.backdropFilter = 'blur(10px)';
            } else {
                header.style.background = '#ffffff';
                header.style.backdropFilter = 'none';
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
    
    // Add loading animation for images
    const images = document.querySelectorAll('img');
    images.forEach(img => {
        img.addEventListener('load', function() {
            this.style.opacity = '1';
        });
        
        // Set initial opacity
        img.style.opacity = '0';
        img.style.transition = 'opacity 0.3s ease';
    });
    
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
            
            // Handle mobile logout
            const mobileLogoutBtn = document.getElementById('mobile-logout-btn');
            if (mobileLogoutBtn) {
                mobileLogoutBtn.addEventListener('click', function(e) {
                    e.preventDefault();
                    const logoutForm = document.querySelector('form[action*="Logout"]');
                    if (logoutForm) {
                        logoutForm.submit();
                    }
                });
            }
            
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
