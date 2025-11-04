document.addEventListener('DOMContentLoaded', function () {
    console.log('🔍 Packages.js loaded - Version 2.0');
    initializePackageItemsManagement();
    initializeFormHandlers();

    // Debug ViewBag.Items
    setTimeout(function () {
        debugViewBagItems();
        debugAllSelects();
    }, 500);
});

function initializePackageItemsManagement() {
    console.log('🔧 Initializing package items management');

    // Add Package Item functionality
    document.addEventListener('click', function (e) {
        var addButton = e.target.closest('.add-package-item');
        if (addButton) {
            e.preventDefault();
            console.log('➕ Add item button clicked');
            var container = addButton.closest('.card-body').querySelector('[id^="packageItems"], [id^="editPackageItems-"]');
            if (container) {
                addPackageItemRow(container);
            }
        }
    });

    // Remove Package Item functionality
    document.addEventListener('click', function (e) {
        var removeButton = e.target.closest('.remove-item');
        if (removeButton) {
            e.preventDefault();
            console.log('➖ Remove item button clicked');
            var row = removeButton.closest('.package-item-row');
            var container = row.parentElement;

            if (container.querySelectorAll('.package-item-row').length > 1) {
                row.remove();
                reindexPackageItems(container.closest('form'));
            } else {
                alert('At least one item is required for the package.');
            }
        }
    });

    // Item selection change handler
    document.addEventListener('change', function (e) {
        if (e.target.matches('select[name*="ItemId"], .item-select')) {
            console.log('📝 Item select changed:', e.target.name, '=', e.target.value);
            updateItemStock(e.target);
        }
    });

    // Quantity input change handler - IMPORTANT: Prevent forced 0
    document.addEventListener('input', function (e) {
        if (e.target.matches('input[name*="ItemQuantity"], .item-quantity')) {
            console.log('🔢 Quantity input changed:', e.target.name, '=', e.target.value);

            // Don't automatically validate - let user type
            var value = parseInt(e.target.value) || 0;
            if (value < 0) {
                e.target.value = '0';
            }
        }
    });

    // Initialize existing selects
    setTimeout(function () {
        console.log('🚀 Initializing existing item selects...');
        document.querySelectorAll('select[name*="ItemId"], .item-select').forEach(function (select, index) {
            if (select.value && select.selectedIndex > 0) {
                console.log(`🎯 Initializing select ${index} with value: ${select.value}`);
                updateItemStock(select);
            }
        });
    }, 300);
}

function updateItemStock(selectElement) {
    console.log('📊 updateItemStock called for:', selectElement.name);

    var selectedOption = selectElement.options[selectElement.selectedIndex];
    var row = selectElement.closest('.package-item-row');
    var stockInfo = row.querySelector('.available-stock');
    var quantityInput = row.querySelector('input[name*="ItemQuantity"], .item-quantity');

    console.log('Selected option:', selectedOption ? selectedOption.textContent.trim() : 'none');

    if (!selectedOption || selectedOption.value === '') {
        console.log('❌ No option selected - clearing stock info');
        if (stockInfo) {
            stockInfo.textContent = '';
            stockInfo.className = 'available-stock form-text text-muted';
        }
        if (quantityInput) {
            quantityInput.disabled = false;
            quantityInput.removeAttribute('max');
            if (!quantityInput.value || quantityInput.value === '0') {
                quantityInput.value = '1';
            }
        }
        return;
    }

    // Get stock from data attribute
    var stockAttr = selectedOption.getAttribute('data-stock');
    var stock = parseInt(stockAttr) || 0;

    console.log('📈 Stock data:', {
        'data-stock attribute': stockAttr,
        'parsed stock': stock,
        'option text': selectedOption.textContent.trim()
    });

    // Update stock info display
    if (stockInfo) {
        if (stock > 0) {
            stockInfo.textContent = `Available: ${stock}`;
            stockInfo.className = 'available-stock form-text text-success';
            console.log('✅ Stock available:', stock);
        } else {
            stockInfo.textContent = 'Out of stock';
            stockInfo.className = 'available-stock form-text text-danger';
            console.log('❌ Out of stock');
        }
    }

    // Update quantity input
    if (quantityInput) {
        var currentValue = parseInt(quantityInput.value) || 0;

        if (stock > 0) {
            quantityInput.disabled = false;
            quantityInput.setAttribute('max', stock);

            // Only change value if it's 0 or invalid
            if (currentValue <= 0) {
                quantityInput.value = '1';
                console.log('🔧 Set quantity to 1 (was 0 or invalid)');
            } else if (currentValue > stock) {
                quantityInput.value = stock.toString();
                console.log('🔧 Reduced quantity to max stock:', stock);
            }

            console.log('✅ Quantity input enabled, max:', stock, 'current:', quantityInput.value);
        } else {
            quantityInput.disabled = true;
            quantityInput.value = '0';
            quantityInput.setAttribute('max', '0');
            console.log('❌ Quantity input disabled (no stock)');
        }
    }
}

function addPackageItemRow(container) {
    console.log('➕ Adding package item row');
    var existingRows = container.querySelectorAll('.package-item-row');
    if (existingRows.length === 0) {
        console.error('❌ No existing rows found to clone');
        return;
    }

    var newIndex = existingRows.length;
    var templateRow = existingRows[0].cloneNode(true);

    // Clear values and update names
    templateRow.querySelectorAll('select, input').forEach(function (element) {
        if (element.name.includes('ItemId')) {
            element.name = element.name.replace(/\[\d+\]/, '[' + newIndex + ']');
            element.value = '';
            element.selectedIndex = 0;
        } else if (element.name.includes('ItemQuantity')) {
            element.name = element.name.replace(/\[\d+\]/, '[' + newIndex + ']');
            element.value = '1';
            element.disabled = false;
            element.removeAttribute('max');
        }
    });

    // Clear stock info
    var stockInfo = templateRow.querySelector('.available-stock');
    if (stockInfo) {
        stockInfo.textContent = '';
        stockInfo.className = 'available-stock form-text text-muted';
    }

    container.appendChild(templateRow);
    console.log('✅ New row added with index:', newIndex);
}

function initializeFormHandlers() {
    // Form submission handler
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (form.id && (form.id.startsWith('editPackageForm-') || form.id === 'addPackageForm')) {
            console.log('📤 Form submitting, reindexing items...');
            reindexPackageItems(form);
        }
    });
}

function reindexPackageItems(form) {
    if (!form) {
        console.error('❌ No form provided for reindexing');
        return;
    }

    console.log('🔢 Reindexing package items in form:', form.id);
    var rows = form.querySelectorAll('.package-item-row');
    rows.forEach(function (row, index) {
        row.querySelectorAll('select, input').forEach(function (element) {
            if (element.name && element.name.includes('PackageItems')) {
                var oldName = element.name;
                element.name = element.name.replace(/\[\d+\]/, '[' + index + ']');
                console.log('📝 Renamed:', oldName, '->', element.name);
            }
        });
    });
}

// Debug functions
function debugViewBagItems() {
    console.log('🔍 DEBUG: ViewBag.Items analysis');

    var selects = document.querySelectorAll('select[name*="ItemId"]');
    selects.forEach(function (select, selectIndex) {
        console.log(`Select ${selectIndex}:`, select.name);

        Array.from(select.options).forEach(function (option, optIndex) {
            if (option.value) {
                console.log(`  Option ${optIndex}:`, {
                    value: option.value,
                    text: option.textContent.trim(),
                    'data-stock': option.getAttribute('data-stock'),
                    selected: option.selected
                });
            }
        });
    });
}

function debugAllSelects() {
    console.log('🔍 DEBUG: Current select states');

    document.querySelectorAll('select[name*="ItemId"]').forEach(function (select, index) {
        var selectedOption = select.options[select.selectedIndex];
        console.log(`Select ${index}:`, {
            name: select.name,
            selectedIndex: select.selectedIndex,
            selectedValue: select.value,
            selectedText: selectedOption ? selectedOption.textContent.trim() : 'none',
            selectedStock: selectedOption ? selectedOption.getAttribute('data-stock') : 'none'
        });
    });
}

// Manual trigger for debugging
window.debugPackages = function () {
    debugViewBagItems();
    debugAllSelects();
};
