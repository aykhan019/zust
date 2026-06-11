"use strict";

// A single shared SignalR connection for the whole app. This file is included both by the
// layout (so every page has a connection for global notification sounds / badge updates) and
// by individual pages that register their own handlers. The guard below makes it safe to load
// more than once: the connection is created and started only the first time, and every later
// include simply reuses the same `connection` object.
(function () {
    if (window.connection) {
        return; // already initialised by an earlier include on this page
    }

    var conn = new signalR.HubConnectionBuilder().withUrl("/userhub").build();

    // Expose it as the global `connection` that page scripts reference.
    window.connection = conn;

    function startConnection() {
        conn.start()
            .then(function () {
                console.log("Connected to chathub");
            })
            .catch(function (err) {
                console.error(err.toString());
                console.log("Connection lost. Reconnecting...");
                setTimeout(startConnection, 5000); // Try reconnecting after 5 seconds
            });
    }

    // Reconnect on error.
    conn.onclose(function (error) {
        console.log("Connection closed. Reason: " + (error && error.message));
        console.log("Reconnecting...");
        setTimeout(startConnection, 5000); // Try reconnecting after 5 seconds
    });

    startConnection();
})();
