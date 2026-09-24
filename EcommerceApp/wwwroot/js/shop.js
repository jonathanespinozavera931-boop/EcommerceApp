// ============================================
// NEXHARD - SHOP: Agregar al carrito (AJAX)
// ============================================

document.addEventListener('DOMContentLoaded', function () {
    // Conectar todos los botones "Agregar al carrito"
    document.querySelectorAll('.add-to-cart-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();

            const productId = this.dataset.id;
            const productName = this.dataset.name;

            // Obtener cantidad seleccionada en el card
            const qtySpan = document.getElementById(`qty-${productId}`);
            const quantity = qtySpan ? parseInt(qtySpan.textContent) : 1;

            addToCart(productId, quantity, productName, this);
        });
    });
});

// ============================================
// Función principal: agregar al carrito
// ============================================
function addToCart(productId, quantity, productName, button) {
    const token = getAntiForgeryToken();

    const formData = new FormData();
    formData.append('productId', productId);
    formData.append('quantity', quantity);
    formData.append('__RequestVerificationToken', token);

    // Deshabilitar botón temporalmente
    if (button) {
        button.disabled = true;
        const originalHTML = button.innerHTML;
        button.innerHTML = '<span>⏳ Agregando...</span>';

        setTimeout(() => {
            button.disabled = false;
            button.innerHTML = originalHTML;
        }, 1000);
    }

    fetch('/Cart/Add', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Feedback visual en el botón
                if (button) {
                    const originalHTML = button.innerHTML;
                    button.classList.add('rog-added');
                    button.innerHTML = '<span>✓ Agregado</span>';

                    setTimeout(() => {
                        button.classList.remove('rog-added');
                        button.innerHTML = originalHTML;
                    }, 1500);
                }

                // Toast
                if (typeof showToast === 'function') {
                    showToast(`"${productName}" agregado al carrito`, 'success');
                }

                // Actualizar badge del navbar
                if (typeof updateCartBadge === 'function') {
                    updateCartBadge();
                }

                // Animación del ícono del carrito
                animateCartIcon();
            } else {
                if (typeof showToast === 'function') {
                    showToast(data.message || 'Error al agregar', 'error');
                } else {
                    alert(data.message);
                }
            }
        })
        .catch(err => {
            console.error('Error:', err);
            if (typeof showToast === 'function') {
                showToast('Error de conexión', 'error');
            }
        });
}

// ============================================
// Obtener token anti-forgery
// ============================================
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

// ============================================
// Animación del ícono del carrito
// ============================================
function animateCartIcon() {
    const cartIcon = document.querySelector('.rog-nav-cart') || document.querySelector('.cart-pill');
    if (!cartIcon) return;

    cartIcon.style.transition = 'transform 0.3s';
    cartIcon.style.transform = 'scale(1.3)';

    setTimeout(() => {
        cartIcon.style.transform = 'scale(1)';
    }, 400);
}