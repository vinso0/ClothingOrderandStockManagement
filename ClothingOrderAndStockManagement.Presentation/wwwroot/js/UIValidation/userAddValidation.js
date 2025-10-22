// userAddValidation.js
function initializeUserAddValidation() {
    const form = document.getElementById('addUserForm');

    if (!form) return;

    // Get form inputs
    const username = document.getElementById('addUserName');
    const email = document.getElementById('addEmail');
    const password = document.getElementById('addPassword');
    const role = document.getElementById('addRole');

    // Get error spans
    const usernameError = document.getElementById('addUserNameError');
    const emailError = document.getElementById('addEmailError');
    const passwordError = document.getElementById('addPasswordError');
    const roleError = document.getElementById('addRoleError');

    // Real-time validation on input
    username.addEventListener('input', function () {
        validateUsername();
    });

    username.addEventListener('blur', function () {
        validateUsername();
    });

    email.addEventListener('input', function () {
        validateEmail();
    });

    email.addEventListener('blur', function () {
        validateEmail();
    });

    password.addEventListener('input', function () {
        validatePassword();
    });

    password.addEventListener('blur', function () {
        validatePassword();
    });

    role.addEventListener('change', function () {
        validateRole();
    });

    role.addEventListener('blur', function () {
        validateRole();
    });

    // Validation functions
    function validateUsername() {
        const value = username.value.trim();

        if (value === '') {
            setInvalid(username, usernameError, 'Username is required.');
            return false;
        }

        if (value.length < 3) {
            setInvalid(username, usernameError, 'Username must be at least 3 characters long.');
            return false;
        }

        if (value.length > 50) {
            setInvalid(username, usernameError, 'Username cannot exceed 50 characters.');
            return false;
        }

        if (!/^[a-zA-Z0-9_.-]+$/.test(value)) {
            setInvalid(username, usernameError, 'Username is invalid.');
            return false;
        }

        setValid(username, usernameError);
        return true;
    }

    function validateEmail() {
        const value = email.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (value === '') {
            setInvalid(email, emailError, 'Email is required.');
            return false;
        }

        if (!emailRegex.test(value)) {
            setInvalid(email, emailError, 'Please enter a valid email address.');
            return false;
        }

        if (value.length > 100) {
            setInvalid(email, emailError, 'Email cannot exceed 100 characters.');
            return false;
        }

        setValid(email, emailError);
        return true;
    }

    function validatePassword() {
        const value = password.value;

        if (value === '') {
            setInvalid(password, passwordError, 'Password is required.');
            return false;
        }

        if (value.length < 6) {
            setInvalid(password, passwordError, 'Password must be at least 6 characters long.');
            return false;
        }

        if (value.length > 100) {
            setInvalid(password, passwordError, 'Password cannot exceed 100 characters.');
            return false;
        }

        // Optional: Check for password strength
        const hasUpperCase = /[A-Z]/.test(value);
        const hasLowerCase = /[a-z]/.test(value);
        const hasNumbers = /\d/.test(value);

        if (!hasUpperCase || !hasLowerCase || !hasNumbers) {
            setInvalid(password, passwordError, 'Password must contain at least one uppercase letter, one lowercase letter, and one number.');
            return false;
        }

        setValid(password, passwordError);
        return true;
    }

    function validateRole() {
        const value = role.value;

        if (value === '' || value === null || value === undefined) {
            setInvalid(role, roleError, 'Please select a role.');
            return false;
        }

        setValid(role, roleError);
        return true;
    }

    function setInvalid(field, errorSpan, message) {
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');
        errorSpan.textContent = message;
        errorSpan.style.display = 'block';
    }

    function setValid(field, errorSpan) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
        errorSpan.textContent = '';
        errorSpan.style.display = 'none';
    }

    function clearValidation() {
        // Clear all validation states
        username.classList.remove('is-invalid', 'is-valid');
        email.classList.remove('is-invalid', 'is-valid');
        password.classList.remove('is-invalid', 'is-valid');
        role.classList.remove('is-invalid', 'is-valid');

        // Clear all error messages
        usernameError.textContent = '';
        emailError.textContent = '';
        passwordError.textContent = '';
        roleError.textContent = '';

        // Reset form
        form.reset();
    }

    // Form submission validation
    form.addEventListener("submit", function (event) {
        event.preventDefault();
        event.stopPropagation();

        // Validate all fields
        const isUsernameValid = validateUsername();
        const isEmailValid = validateEmail();
        const isPasswordValid = validatePassword();
        const isRoleValid = validateRole();

        // Check if form is valid
        if (isUsernameValid && isEmailValid && isPasswordValid && isRoleValid) {
            // Form is valid, submit it
            form.submit();
        } else {
            // Focus on first invalid field
            if (!isUsernameValid) {
                username.focus();
            } else if (!isEmailValid) {
                email.focus();
            } else if (!isPasswordValid) {
                password.focus();
            } else if (!isRoleValid) {
                role.focus();
            }
        }
    });

    // Clear validation when modal is closed
    const modal = document.getElementById('addUserModal');
    if (modal) {
        modal.addEventListener('hidden.bs.modal', function () {
            clearValidation();
        });
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializeUserAddValidation();
});