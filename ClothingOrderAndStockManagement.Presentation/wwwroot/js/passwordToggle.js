// passwordToggle.js
function initializePasswordToggle() {
    // Find all password inputs on the page
    const passwordInputs = document.querySelectorAll('input[type="password"]');

    passwordInputs.forEach(function (passwordInput) {
        // Check if toggle button already exists
        if (passwordInput.parentElement.querySelector('.password-toggle-btn')) {
            return;
        }

        // Wrap the input in a position-relative div if not already wrapped
        let wrapper = passwordInput.parentElement;
        if (!wrapper.classList.contains('position-relative')) {
            const newWrapper = document.createElement('div');
            newWrapper.className = 'position-relative';
            passwordInput.parentNode.insertBefore(newWrapper, passwordInput);
            newWrapper.appendChild(passwordInput);
            wrapper = newWrapper;
        }

        // Create the toggle button
        const toggleButton = document.createElement('button');
        toggleButton.type = 'button';
        toggleButton.className = 'password-toggle-btn';
        toggleButton.innerHTML = `
            <svg class="eye-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                <circle cx="12" cy="12" r="3"></circle>
            </svg>
        `;
        toggleButton.setAttribute('aria-label', 'Toggle password visibility');
        toggleButton.setAttribute('tabindex', '-1');

        // Add click event
        toggleButton.addEventListener('click', function (e) {
            e.preventDefault();

            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                toggleButton.innerHTML = `
                    <svg class="eye-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                        <line x1="1" y1="1" x2="23" y2="23"></line>
                    </svg>
                `;
                toggleButton.setAttribute('aria-label', 'Hide password');
            } else {
                passwordInput.type = 'password';
                toggleButton.innerHTML = `
                    <svg class="eye-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                        <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                `;
                toggleButton.setAttribute('aria-label', 'Show password');
            }
        });

        // Insert the button after the input
        wrapper.appendChild(toggleButton);
    });
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    initializePasswordToggle();
});

// Re-initialize when modals are shown (for dynamically loaded content)
document.addEventListener('shown.bs.modal', function () {
    initializePasswordToggle();
});