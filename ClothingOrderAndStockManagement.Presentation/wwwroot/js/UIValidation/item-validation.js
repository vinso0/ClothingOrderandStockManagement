document.addEventListener('DOMContentLoaded', function () {
    // Handle create item form validation
    const createForm = document.getElementById('createItemForm');
    if (createForm) {
        validateItemForm(createForm);
    }

    // Handle edit item forms validation (multiple forms with unique IDs)
    const editForms = document.querySelectorAll('[id^="editItemForm-"]');
    editForms.forEach(form => {
        validateItemForm(form);
    });
});

function validateItemForm(form) {
    const formId = form.id;
    const isEditForm = formId.includes('edit');
    const itemId = isEditForm ? formId.split('-')[1] : '';

    // Get form elements with proper IDs
    const categorySelect = form.querySelector(isEditForm ? `#editItemCategorySelect-${itemId}` : '#itemCategorySelect');
    const sizeInput = form.querySelector(isEditForm ? `#editSizeInput-${itemId}` : '#sizeInput');
    const colorInput = form.querySelector(isEditForm ? `#editColorInput-${itemId}` : '#colorInput');
    const quantityInput = form.querySelector(isEditForm ? `#editQuantityInput-${itemId}` : '#quantityInput');

    form.addEventListener('submit', function (e) {
        let isValid = true;

        // Clear previous validation errors
        clearValidationErrors(form);

        // Validate category selection
        if (!categorySelect.value) {
            showValidationError(categorySelect, 'Please select a category.');
            isValid = false;
        }

        // Validate quantity
        if (!quantityInput.value || quantityInput.value < 0) {
            showValidationError(quantityInput, 'Quantity must be a positive number.');
            isValid = false;
        }

        // Validate size (optional but if provided, must be reasonable)
        if (sizeInput.value && sizeInput.value.trim().length > 50) {
            showValidationError(sizeInput, 'Size must be less than 50 characters.');
            isValid = false;
        }

        // Validate color (optional but if provided, must be reasonable)
        if (colorInput.value && colorInput.value.trim().length > 50) {
            showValidationError(colorInput, 'Color must be less than 50 characters.');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
        }
    });

    // Real-time validation for quantity
    quantityInput.addEventListener('input', function () {
        if (this.value && this.value < 0) {
            showValidationError(this, 'Quantity cannot be negative.');
        } else {
            clearFieldValidationError(this);
        }
    });
}

function showValidationError(field, message) {
    field.classList.add('is-invalid');

    // Find or create error span
    let errorSpan = field.parentNode.querySelector('.text-danger');
    if (!errorSpan) {
        errorSpan = document.createElement('span');
        errorSpan.classList.add('text-danger');
        field.parentNode.appendChild(errorSpan);
    }
    errorSpan.textContent = message;
}

function clearFieldValidationError(field) {
    field.classList.remove('is-invalid');
    const errorSpan = field.parentNode.querySelector('.text-danger');
    if (errorSpan && !errorSpan.getAttribute('data-server-error')) {
        errorSpan.textContent = '';
    }
}

function clearValidationErrors(form) {
    const invalidFields = form.querySelectorAll('.is-invalid');
    invalidFields.forEach(field => {
        field.classList.remove('is-invalid');
    });

    const errorSpans = form.querySelectorAll('.text-danger');
    errorSpans.forEach(span => {
        if (!span.getAttribute('data-server-error')) {
            span.textContent = '';
        }
    });
}
