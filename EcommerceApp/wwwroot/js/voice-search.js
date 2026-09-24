// ============================================================
// NEXHARD - Búsqueda por Voz y Comandos (Web Speech API)
// ============================================================

document.addEventListener('DOMContentLoaded', function () {
    const voiceBtn = document.getElementById('voiceSearchBtn');
    const searchInput = document.getElementById('irisSearch');
    const searchBtn = document.getElementById('irisSearchBtn');

    // Verificar soporte del navegador
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

    if (!SpeechRecognition) {
        console.warn('Web Speech API no soportada en este navegador.');
        if (voiceBtn) {
            voiceBtn.style.display = 'none'; // Ocultar botón si no hay soporte
        }
        return;
    }

    // --- Configuración del Reconocimiento de Voz ---
    const recognition = new SpeechRecognition();
    recognition.lang = 'es-ES'; // Configurar español
    recognition.continuous = false; // Una sola frase por vez
    recognition.interimResults = false; // Solo resultados finales
    recognition.maxAlternatives = 1;

    // Variable para saber si estamos buscando o dando un comando
    let isListening = false;

    // --- Funciones de Feedback Visual ---
    function setListeningState(listening) {
        isListening = listening;
        if (listening) {
            voiceBtn.style.background = '#FF003C'; // Rojo al escuchar
            voiceBtn.textContent = '🔴';
            voiceBtn.title = 'Escuchando...';
            // Feedback auditivo
            hablar('Te escucho');
        } else {
            voiceBtn.style.background = 'var(--cyber-cyan)'; // Vuelve al color normal
            voiceBtn.textContent = '🎤';
            voiceBtn.title = 'Buscar por voz';
        }
    }

    // --- Función de Síntesis de Voz (Text-to-Speech) ---
    function hablar(texto) {
        if ('speechSynthesis' in window) {
            // Cancelar cualquier habla anterior para evitar solapamientos
            window.speechSynthesis.cancel();
            const utterance = new SpeechSynthesisUtterance(texto);
            utterance.lang = 'es-ES';
            utterance.rate = 1.0; // Velocidad normal
            utterance.pitch = 1.0; // Tono normal
            window.speechSynthesis.speak(utterance);
        }
    }

    // --- Iniciar el Reconocimiento al hacer clic en el botón ---
    voiceBtn.addEventListener('click', function () {
        if (isListening) {
            recognition.stop(); // Si ya está escuchando, detener
        } else {
            try {
                recognition.start();
            } catch (e) {
                // El error más común es que ya esté iniciado
                console.log("Error al iniciar reconocimiento:", e);
            }
        }
    });

    // --- Eventos del Reconocimiento ---
    recognition.onstart = function () {
        console.log('Reconocimiento de voz iniciado.');
        setListeningState(true);
    };

    recognition.onend = function () {
        console.log('Reconocimiento de voz detenido.');
        setListeningState(false);
    };

    recognition.onerror = function (event) {
        console.error('Error en reconocimiento de voz:', event.error);
        setListeningState(false);
        if (event.error === 'not-allowed') {
            alert('Por favor, permite el acceso al micrófono para usar la búsqueda por voz.');
        }
    };

    recognition.onresult = function (event) {
        const transcript = event.results[0][0].transcript.trim();
        console.log('Texto reconocido:', transcript);

        // Convertir a minúsculas para comparar comandos
        const lowerTranscript = transcript.toLowerCase();

        // --- Lógica de Comandos de Voz ---
        if (lowerTranscript.includes('buscar') || lowerTranscript.includes('busca')) {
            // Extraer el término de búsqueda después de la palabra "buscar"
            let termino = lowerTranscript.replace('buscar', '').replace('busca', '').trim();
            if (termino) {
                searchInput.value = termino;
                realizarBusqueda(termino);
                hablar('Buscando ' + termino);
            } else {
                hablar('¿Qué quieres buscar?');
            }
        } else if (lowerTranscript.includes('ir a inicio') || lowerTranscript.includes('inicio')) {
            hablar('Yendo a inicio');
            window.location.href = '/';
        } else if (lowerTranscript.includes('mis pedidos')) {
            hablar('Yendo a mis pedidos');
            window.location.href = '/Orders/MyOrders';
        } else if (lowerTranscript.includes('carrito') || lowerTranscript.includes('mi carrito')) {
            hablar('Abriendo tu carrito');
            window.location.href = '/Cart';
        } else {
            // Si no es un comando, lo tratamos como una búsqueda directa
            searchInput.value = transcript;
            realizarBusqueda(transcript);
            hablar('Buscando ' + transcript);
        }
    };

    // --- Función Auxiliar para Realizar la Búsqueda ---
    function realizarBusqueda(termino) {
        // Si estamos en la página de productos, filtramos en el cliente.
        // Si no, redirigimos a la página de productos con el parámetro de búsqueda.
        if (window.location.pathname.toLowerCase().includes('/products')) {
            const localSearchInput = document.getElementById('localSearch');
            if (localSearchInput) {
                localSearchInput.value = termino;
                // Disparar el evento input para que se aplique el filtro
                localSearchInput.dispatchEvent(new Event('input', { bubbles: true }));
            }
        } else {
            window.location.href = '/Products?search=' + encodeURIComponent(termino);
        }
    }

    // --- Síntesis de Voz al Pasar el Mouse sobre un Producto (Opcional) ---
    // Puedes añadir esta funcionalidad si quieres que lea los productos al pasar el mouse.
    // document.querySelectorAll('.iris-catalog-card').forEach(card => {
    //     card.addEventListener('mouseenter', function() {
    //         const name = this.dataset.name;
    //         const price = this.querySelector('.iris-catalog-card-price')?.textContent;
    //         if (name) {
    //             hablar(`${name}. Precio: ${price || 'no disponible'}`);
    //         }
    //     });
    // });
});