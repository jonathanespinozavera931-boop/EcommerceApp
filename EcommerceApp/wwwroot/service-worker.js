// ============================================================
// NEXHARD - Service Worker (PWA) - Versión 2
// ============================================================

const CACHE_NAME = 'nexhard-v2-' + Date.now();
const urlsToCache = [
    '/css/white-tech.css',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/js/cart-badge.js'
];

// INSTALACIÓN
self.addEventListener('install', event => {
    self.skipWaiting(); // Activar inmediatamente
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache).catch(() => { }))
    );
});

// ACTIVACIÓN - Limpiar TODAS las cachés viejas
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    // Borrar TODAS las cachés (forzar recarga)
                    return caches.delete(cacheName);
                })
            );
        }).then(() => self.clients.claim()) // Tomar control inmediato
    );
});

// FETCH - Estrategia: HTML siempre del servidor, assets desde caché
self.addEventListener('fetch', event => {
    const request = event.request;
    const url = new URL(request.url);

    // Solo procesar GET
    if (request.method !== 'GET') return;

    // Ignorar llamadas a otros dominios (Google, Unsplash, etc.)
    if (url.origin !== self.location.origin) return;

    // ══════════ HTML SIEMPRE DESDE EL SERVIDOR ══════════
    if (request.headers.get('accept')?.includes('text/html')) {
        event.respondWith(
            fetch(request)
                .then(response => {
                    // No cachear HTML
                    return response;
                })
                .catch(() => {
                    // Si falla, intentar desde caché (offline)
                    return caches.match(request);
                })
        );
        return;
    }

    // ══════════ CSS/JS/IMÁGENES: caché primero ══════════
    event.respondWith(
        caches.match(request)
            .then(cachedResponse => {
                if (cachedResponse) return cachedResponse;

                return fetch(request).then(response => {
                    // Cachear solo si es 200 OK
                    if (response.status === 200) {
                        const responseClone = response.clone();
                        caches.open(CACHE_NAME).then(cache => {
                            cache.put(request, responseClone);
                        });
                    }
                    return response;
                });
            })
            .catch(() => fetch(request))
    );
});

// PUSH NOTIFICATIONS (sin cambios)
self.addEventListener('push', event => {
    if (!event.data) return;

    const data = event.data.json();
    const options = {
        body: data.body || 'Nueva notificación de NexHard',
        icon: data.icon || '/images/logo-nexhard.png',
        badge: data.badge || '/images/logo-nexhard.png',
        data: data.data || { url: '/' },
        vibrate: [200, 100, 200]
    };

    event.waitUntil(
        self.registration.showNotification(data.title || 'NexHard', options)
    );
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    const url = event.notification.data?.url || '/';

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true })
            .then(windowClients => {
                for (let client of windowClients) {
                    if (client.url === url && 'focus' in client) {
                        return client.focus();
                    }
                }
                if (clients.openWindow) return clients.openWindow(url);
            })
    );
});