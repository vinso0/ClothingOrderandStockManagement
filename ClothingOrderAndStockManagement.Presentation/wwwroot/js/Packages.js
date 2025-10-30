document.addEventListener('DOMContentLoaded', function () {
    let itemIndex = 0;
    const packageItemsContainer = document.getElementById('packageItems');
    const addItemButton = document.getElementById('addPackageItem');

    // Initialize item index based on existing items
    const existingItems = packageItemsContainer.querySelectorAll('.package-item-row');
    itemIndex = existingItems.length;

    // Add item functionality
    if (addItemButton) {
        addItemButton.addEventListener('click', function () {
            const itemsSelect = document.querySelector('select[name$=".ItemId"]');
            if (!itemsSelect) return;

            const optionsHtml = Array.from(itemsSelect.options)
                .map(option => `<option value="${option.value}">${option.text}</option>`)
                .join('');

            const newItemHtml = `
                <div class="package-item-row row mb-2">
                    <div class="col-md-6">
                        <select name="PackageItems[${itemIndex}].ItemId" class="form-select" required>
                            ${optionsHtml}
                        </select>
                    </div>
                    <div class="col-md-4">
                        <input type="number" name="PackageItems[${itemIndex}].ItemQuantity" placeholder="Quantity" class="form-control" min="1" required />
                    </div>
                    <div class="col-md-2">
                        <button type="button" class="btn btn-danger btn-sm remove-item">Remove</button>
                    </div>
                </div>
            `;

            packageItemsContainer.insertAdjacentHTML('beforeend', newItemHtml);
            itemIndex++;

            // Reattach remove event listeners
            attachRemoveEventListeners();
        });
    }

    // Remove item functionality
    function attachRemoveEventListeners() {
        const removeButtons = document.querySelectorAll('.remove-item');
        removeButtons.forEach(button => {
            button.removeEventListener('click', handleRemoveClick);
            button.addEventListener('click', handleRemoveClick);
        });
    }

    function handleRemoveClick(event) {
        const itemRow = event.target.closest('.package-item-row');
        const remainingRows = document.querySelectorAll('.package-item-row');

        if (remainingRows.length > 1) {
            itemRow.remove();
            reindexItems();
        } else {
            alert('At least one item is required for a package.');
        }
    }

    // Reindex items after removal
    function reindexItems() {
        const itemRows = document.querySelectorAll('.package-item-row');
        itemRows.forEach((row, index) => {
            const select = row.querySelector('select');
            const input = row.querySelector('input');

            if (select) {
                select.name = `PackageItems[${index}].ItemId`;
            }
            if (input) {
                input.name = `PackageItems[${index}].ItemQuantity`;
            }
        });
        itemIndex = itemRows.length;
    }

    // Initialize remove event listeners
    attachRemoveEventListeners();

    // Form validation
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (event) {
            const itemSelects = form.querySelectorAll('select[name$=".ItemId"]');
            const itemQuantities = form.querySelectorAll('input[name$=".ItemQuantity"]');

            let hasValidItems = false;

            for (let i = 0; i < itemSelects.length; i++) {
                if (itemSelects[i].value && itemQuantities[i].value && parseInt(itemQuantities[i].value) > 0) {
                    hasValidItems = true;
                    break;
                }
            }

            if (!hasValidItems) {
                event.preventDefault();
                alert('Please add at least one valid item to the package.');
                return false;
            }

            // Check for duplicate items
            const selectedItems = Array.from(itemSelects).map(select => select.value).filter(value => value);
            const uniqueItems = new Set(selectedItems);

            if (selectedItems.length !== uniqueItems.size) {
                event.preventDefault();
                alert('Duplicate items are not allowed in a package.');
                return false;
            }
        });
    }
});
