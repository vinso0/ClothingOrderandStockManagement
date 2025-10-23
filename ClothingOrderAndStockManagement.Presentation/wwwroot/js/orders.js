let packageCount = 1;
let packagesData = []; // This will be populated from the view

// Initialize packages data from the view
function initializePackagesData(packages) {
    packagesData = packages;
}

function addPackageRow() {
    const container = document.getElementById('packageContainer');
    const idx = packageCount;
    const row = document.createElement('div');
    row.className = 'package-item';

    // Build options HTML from packages data
    let optionsHtml = '<option value="">Select Package</option>';
    packagesData.forEach(pkg => {
        optionsHtml += `<option value="${pkg.PackagesId}" data-price="${pkg.Price}">${pkg.PackageName} - ₱${pkg.Price.toFixed(2)}</option>`;
    });

    row.innerHTML = `
        <div class="row g-3">
            <div class="col-md-5">
                <label class="form-label">Package</label>
                <select class="form-select package-select" name="OrderPackages[${idx}].PackagesId" required onchange="updatePrice(this, ${idx})">
                    ${optionsHtml}
                </select>
            </div>
            <div class="col-md-3">
                <label class="form-label">Quantity</label>
                <input type="number" class="form-control quantity-input"
                       name="OrderPackages[${idx}].Quantity" min="1" value="1" required
                       onchange="updatePrice(this, ${idx})" />
            </div>
            <div class="col-md-3">
                <label class="form-label">Subtotal</label>
                <input type="text" class="form-control subtotal-display" id="subtotal-${idx}" readonly value="₱0.00" />
                <input type="hidden" name="OrderPackages[${idx}].PriceAtPurchase" id="price-${idx}" value="0" />
            </div>
            <div class="col-md-1 d-flex align-items-end">
                <button type="button" class="btn btn-remove" onclick="removePackageRow(this)" title="Remove package">×</button>
            </div>
        </div>`;
    container.appendChild(row);
    packageCount++;
}

function removePackageRow(button) {
    button.closest('.package-item').remove();
    calculateTotal();
}

function updatePrice(element, index) {
    const row = element.closest('.package-item');
    const select = row.querySelector('.package-select');
    const qty = parseInt(row.querySelector('.quantity-input').value) || 1;
    const selectedOption = select.options[select.selectedIndex];
    const price = parseFloat(selectedOption?.getAttribute('data-price') || 0);
    const subtotal = price * qty;

    document.getElementById(`subtotal-${index}`).value = '₱' + subtotal.toFixed(2);
    document.getElementById(`price-${index}`).value = price.toFixed(2);

    calculateTotal();
}

function calculateTotal() {
    let total = 0;
    document.querySelectorAll('.subtotal-display').forEach(input => {
        const value = parseFloat(input.value.replace('₱', '').replace(/,/g, '')) || 0;
        total += value;
    });
    document.getElementById('totalAmount').textContent = '₱' + total.toFixed(2);
}

// Fixed: Add null check to prevent error on pages without the form
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('createOrderForm');
    if (form) {
        form.addEventListener('submit', function (e) {
            const firstSelect = this.querySelector('.package-select');
            const firstQty = this.querySelector('.quantity-input');
            if (!firstSelect || !firstSelect.value || !firstQty || parseInt(firstQty.value) <= 0) {
                e.preventDefault();
                alert('Please select at least one package and quantity.');
                return;
            }
        });
    }

    // Initialize the first row price calculation
    const firstSelect = document.querySelector('.package-select');
    if (firstSelect) {
        firstSelect.addEventListener('change', function () {
            updatePrice(this, 0);
        });
    }

    const firstQty = document.querySelector('.quantity-input');
    if (firstQty) {
        firstQty.addEventListener('change', function () {
            const select = document.querySelector('.package-select');
            if (select) {
                updatePrice(select, 0);
            }
        });
    }
});
