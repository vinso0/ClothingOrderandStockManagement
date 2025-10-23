let packageCount = 1;

function addPackageRow() {
    const container = document.getElementById('packageContainer');
    const idx = packageCount;
    const row = document.createElement('div');
    row.className = 'package-item';
    row.innerHTML = `
            <div class="row g-3">
                <div class="col-md-5">
                    <label class="form-label">Package</label>
                    <select class="form-select package-select" name="OrderPackages[${idx}].PackagesId" required onchange="updatePrice(this, ${idx})">
                        <option value="">Select Package</option>
                        @foreach (var package in packages)
                        {
                            <option value="@package.PackagesId" data-price="@package.Price">
                                @package.PackageName - ₱@package.Price.ToString("N2")
                            </option>
                        }
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
    const price = parseFloat(select.options[select.selectedIndex]?.getAttribute('data-price') || 0);
    const subtotal = price * qty;

    document.getElementById(`subtotal-${index}`).value = '₱' + subtotal.toFixed(2);
    document.getElementById(`price-${index}`).value = price;

    calculateTotal();
}

function calculateTotal() {
    let total = 0;
    document.querySelectorAll('.subtotal-display').forEach(input => {
        const value = parseFloat(input.value.replace('₱', '').replace(/,/g, '')) || 0;
        total += value;
    });
    document.getElementById('totalAmount').textContent = '₱' + total.toFixed(2);
    calculateRemainingBalance();
}

function calculateRemainingBalance() {
    const totalText = document.getElementById('totalAmount').textContent;
    const total = parseFloat(totalText.replace('₱', '').replace(/,/g, '')) || 0;
    const paid = parseFloat(document.getElementById('paymentAmount').value) || 0;
    const remaining = Math.max(total - paid, 0);
    document.getElementById('remainingBalance').value = '₱' + remaining.toFixed(2);
}

function displayFileName(input, number) {
    const el = document.getElementById('fileName' + number);
    if (input.files && input.files[0]) {
        const file = input.files[0];

        // Validate size (10MB) and type
        if (file.size > 10 * 1024 * 1024) {
            alert('File size must be less than 10MB');
            input.value = '';
            el.textContent = '';
            return;
        }
        const allowed = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (!allowed.includes(file.type)) {
            alert('Only image files (JPG, PNG, GIF) are allowed');
            input.value = '';
            el.textContent = '';
            return;
        }

        el.textContent = '📎 ' + file.name;
        el.style.color = '#28a745';
        el.style.fontWeight = '600';
    } else {
        el.textContent = '';
    }
}

document.getElementById('createOrderForm').addEventListener('submit', function (e) {
    // Basic guard: at least one package line with valid package and quantity
    const firstSelect = this.querySelector('.package-select');
    const firstQty = this.querySelector('.quantity-input');
    if (!firstSelect || !firstSelect.value || !firstQty || parseInt(firstQty.value) <= 0) {
        e.preventDefault();
        alert('Please select at least one package and quantity.');
        return;
    }
});