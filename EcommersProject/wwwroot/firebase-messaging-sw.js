// Firebase Cloud Messaging Service Worker
// Handles background push notifications when the browser tab is closed or backgrounded.
// FCM V1 API · VAPID: BJ41niijwpwxH1H22jQ4EtCCgv0DJjHxbcypgenhzpR24EEeKtjjMXM6TdejCm2b7qa3l5sbM1odCtLYk5QiBCU

importScripts('https://www.gstatic.com/firebasejs/10.12.2/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.12.2/firebase-messaging-compat.js');

firebase.initializeApp({
    apiKey:            "AIzaSyCxELfCxyrDUKdycrZOxQUnlYahVZ4O9gg",
    authDomain:        "alis-veris-85db5.firebaseapp.com",
    projectId:         "alis-veris-85db5",
    storageBucket:     "alis-veris-85db5.firebasestorage.app",
    messagingSenderId: "886662188950",
    appId:             "1:886662188950:web:0aa9c781ef5b404a7397b7"
});

const messaging = firebase.messaging();

// Background message handler — fires when the tab is hidden/closed
messaging.onBackgroundMessage(payload => {
    const notification = payload.notification || {};
    const data         = payload.data       || {};
    const isCall       = data.type === 'call';

    const options = {
        body:    notification.body || '',
        icon:    '/img/favicon.svg',
        badge:   '/img/favicon.svg',
        vibrate: isCall ? [400, 150, 400, 150, 400] : [200],
        data:    { url: data.url || '/', conversationId: data.conversationId, type: data.type },
        tag:     (data.type || 'msg') + '-' + (data.conversationId || ''),
        renotify: true,
        actions: isCall
            ? [
                { action: 'accept',  title: '✅ Ответить'  },
                { action: 'decline', title: '❌ Отклонить' }
              ]
            : []
    };

    self.registration.showNotification(
        notification.title || (isCall ? '📞 Входящий звонок' : 'Новое сообщение'),
        options
    );
});

// Notification click handler
self.addEventListener('notificationclick', event => {
    event.notification.close();
    const d   = event.notification.data || {};
    const url = d.url || '/';

    if (event.action === 'decline' && d.conversationId) {
        fetch(`/Call/Decline?conversationId=${d.conversationId}`, { method: 'GET', credentials: 'include' })
            .catch(() => {});
        return;
    }

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(list => {
            for (const client of list) {
                if (client.url.endsWith(url) && 'focus' in client) return client.focus();
            }
            return clients.openWindow(url);
        })
    );
});
