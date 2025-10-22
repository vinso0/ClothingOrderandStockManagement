// userEditValidation.js
function initializeUserEditValidation(formId) {
    const form = document.getElementById(formId);

    if (!form) return;

    // Get form inputs
    const username = form.querySelector('input[name="UserName"]');
    const email = form.querySelector('input[name="Email"]');
    const password = form.querySelector('input[name="Password"]');
    const role = form.querySelector('select[name="Role"]');

    // Real-time validation on input
    username.addEventListener('input', function () {
        validateUsername();
    });

    email.addEventListener('input', function () {
        validateEmail();
    });

    password.addEventListener('input', function () {
        validatePassword();
    });

    role.addEventListener('change', function () {
        validateRole();
    });

    // Validation functions
    function validateUsername() {
        const value = username.value.trim();

        if (value === '') {
            setInvalid(username, 'Username is required.');
            return false;
        }

        if (value.length < 3) {
            setInvalid(username, 'Username must be at least 3 characters long.');
            return false;
        }

        if (!/^[a-zA-Z0-9_.-]+$/.test(value)) {
            setInvalid(username, 'Username can only contain letters, numbers, dots, hyphens, and underscores.');
            return false;
        }

        setValid(username);
        return true;
    }

    function validateEmail() {
        const value = email.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (value === '') {
            setInvalid(email, 'Email is required.');
            return false;
        }

        if (!emailRegex.test(value)) {
            setInvalid(email, 'Please enter a valid email address.');
            return false;
        }

        setValid(email);
        return true;
    }

    function validatePassword() {
        const value = password.value;

        // Password is optional (can be left blank)
        if (value === '') {
            setValid(password);
            return true;
        }

        if (value.length < 6) {
            setInvalid(password, 'Password must be at least 6 characters long.');
            return false;
        }

        setValid(password);
        return true;
    }

    function validateRole() {
        const value = role.value;

        if (value === '' || value === null) {
            setInvalid(role, 'Please select a role.');
            return false;
        }

        setValid(role);
        return true;
    }

    function setInvalid(field, message) {
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');

        let feedback = field.parentElement.querySelector('.invalid-feedback');

        if (feedback) {
            feedback.textContent = message;
            feedback.classList.add('d-block'); // Bootstrap's display block class
        }
    }

    function setValid(field) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');

        let feedback = field.parentElement.querySelector('.invalid-feedback');

        if (feedback) {
            feedback.classList.remove('d-block');
        }
    }

    // Form submission validation
    form.addEventListener("submit", function (event) {
        // Validate all fields
        const isUsernameValid = validateUsername();
        const isEmailValid = validateEmail();
        const isPasswordValid = validatePassword();
        const isRoleValid = validateRole();

        // Check if form is valid
        if (!isUsernameValid || !isEmailValid || !isPasswordValid || !isRoleValid) {
            // Prevent form submission if invalid
            event.preventDefault();
            event.stopPropagation();

            // Focus on first invalid field
            const firstInvalid = form.querySelector('.is-invalid');
            if (firstInvalid) {
                firstInvalid.focus();
            }
        }
        // If valid, let the form submit naturally (don't call form.submit())
    });
}