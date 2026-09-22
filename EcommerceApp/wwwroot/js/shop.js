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
            const quantity = 1; // Por ahora siempre 1
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

    // Deshabilitar botón temporalmente para evitar doble clic
    if (button) {
        button.disabled = true;
        const originalHTML = button.innerHTML;
        button.innerHTML = '⏳ Agregando...';

        // Restaurar después de la petición
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
                // Mostrar toast de éxito
                if (typeof showToast === 'function') {
                    showToast(`"${productName}" agregado al carrito ✅`, 'success');
                }

                // Actualizar el badge del navbar
                if (typeof updateCartBadge === 'function') {
                    updateCartBadge();
                }

                // Opcional: animar el ícono del carrito
                animateCartIcon();
            } else {
                // Mostrar error
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
// Obtener el token anti-forgery
// ============================================
function getAntiForgeryToken() {
    // Buscar en cualquier formulario de la página
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) return tokenInput.value;

    // Si no hay formulario con token, buscar en meta tags (fallback)
    const meta = document.querySelector('meta[name="csrf-token"]');
    if (meta) return meta.content;

    return '';
}

// ============================================
// Animación del ícono del carrito al agregar
// ============================================
function animateCartIcon() {
    const cartIcon = document.querySelector('.cart-pill');
    if (!cartIcon) return;

    cartIcon.style.transition = 'transform 0.3s';
    cartIcon.style.transform = 'scale(1.3) rotate(-10deg)';

    setTimeout(() => {
        cartIcon.style.transform = 'scale(1) rotate(0)';
    }, 400);
}