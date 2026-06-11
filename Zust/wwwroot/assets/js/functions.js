var toastId = "myToast";

function createToast(text, color) {
    let toast = `
    <div class="position-fixed bottom-0 end-0 p-3" style="z-index: 11">
      <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" style="border: 1px solid ${color}; background-color: white;">
        <div class="toast-header">
          <strong class="me-auto">Zust</strong>
          <small>Now</small>
          <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body" style="color: ${color};">
          ${text}
        </div>
      </div>
    </div>
  `;
    return toast;
}

// Function to show the toast
function showToast(message, color) {
    // Remove existing toast if it exists
    var existingToast = document.getElementById(toastId);
    if (existingToast) {
        existingToast.remove();
    }

    var toastHTML = createToast(message, color);
    document.body.insertAdjacentHTML("beforeend", toastHTML);
    var toast = document.getElementById(toastId);
    var bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    setTimeout(function () {
        toast.style.display = "none";
    }, 6000);

    // Handle close button click event
    var closeButton = toast.querySelector(".btn-close");
    closeButton.addEventListener("click", function () {
        toast.style.display = "none";
    });
}

// Non-blocking error notification. Replaces blocking alert() popups that leaked raw
// server exception text (or "undefined") straight to the user.
function showError(error) {
    var message = "Something went wrong. Please try again.";
    if (error && typeof error.status === "number" && error.status === 0) {
        message = "Network error. Please check your connection and try again.";
    }
    showToast(message, "red");
}

function getDateTimeDifference(dateTime) {
    const requestTime = new Date(dateTime);
    const currentTime = new Date();

    // Calculate the time difference in milliseconds
    const difference = Math.abs(currentTime.getTime() - requestTime.getTime());

    // If the time difference is less than 1 second, return "Right now"
    if (difference < 1000) {
        return "Right now";
    }

    // Convert milliseconds to seconds, minutes, hours, days, months, and years
    const seconds = Math.floor(difference / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);
    const months = Math.floor(days / 30);
    const years = Math.floor(months / 12);

    // Format the result as a string
    if (years > 0) {
        return years + (years === 1 ? " year ago" : " years ago");
    } else if (months > 0) {
        return months + (months === 1 ? " month ago" : " months ago");
    } else if (days > 0) {
        return days + (days === 1 ? " day ago" : " days ago");
    } else if (hours > 0) {
        return hours + (hours === 1 ? " hour ago" : " hours ago");
    } else if (minutes > 0) {
        return minutes + (minutes === 1 ? " minute ago" : " minutes ago");
    } else {
        return seconds + (seconds === 1 ? " second ago" : " seconds ago");
    }
}

function getNoResultHtml(title, message) {
    let content = `
    <div class="empty-icon-container">
      <div class="animation-container">
        <div class="empty-bounce"></div>
        <div class="pebble1"></div>
        <div class="pebble2"></div>
        <div class="pebble3"></div>
      </div>
      <div>
        <h2>${title}</h2>
        <p>${message}</p>
      </div>
    </div>`;
    return content;
}

function getAllFollowings(currentUserId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: `/api/User/GetFollowings?userId=` + currentUserId,
            method: 'GET',
            success: function (data) {
                resolve(data);
            },
            error: function (error) {
                reject(error);
            }
        });
    });
}

function getAllFollowingsCount(currentUserId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: `/api/User/GetFollowingsCount?userId=` + currentUserId,
            method: 'GET',
            success: function (data) {
                resolve(data);
            },
            error: function (error) {
                reject(error);
            }
        });
    });
}

function getAllFollowers(currentUserId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: `/api/User/GetFollowers?userId=` + currentUserId,
            method: 'GET',
            success: function (data) {
                resolve(data);
            },
            error: function (error) {
                reject(error);
            }
        });
    });
}

function getRandomFollowers(currentUserId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: `/api/User/GetRandomFollowers?userId=` + currentUserId,
            method: 'GET',
            success: function (data) {
                resolve(data);
            },
            error: function (error) {
                reject(error);
            }
        });
    });
}


function getAllFollowersCount(currentUserId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: `/api/User/GetFollowersCount?userId=` + currentUserId,
            method: 'GET',
            success: function (data) {
                resolve(data);
            },
            error: function (error) {
                reject(error);
            }
        });
    });
}

async function getAllPostLikeCount(currentUserId) {
    return makeAjaxRequest("/api/Post/GetAllPostsLikeCount?userId=" + currentUserId, "GET");
}

async function getSentFriendRequests(id) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: `/api/FriendRequest/GetSentFriendRequests?userId=` + id,
            method: 'GET',
            success: function (data) {
                resolve(data);
            },
            error: function (error) {
                showError(error);
                reject(false);
            }
        });
    });
}

function sendFriendRequest(receiverId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: '/api/FriendRequest/AddFriendRequest?receiverId=' + receiverId,
            method: 'POST',
            success: async function (friendRequestNotificiationVm) {
                resolve(friendRequestNotificiationVm);
            },
            error: function (error) {
                showError(error);
                reject(false);
            }
        });
    });
}

function cancelFriendRequest(receiverId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: '/api/FriendRequest/CancelFriendRequest?receiverId=' + receiverId,
            method: 'POST',
            success: function () {
                resolve(true);
            },
            error: function (error) {
                showError(error);
                reject(false);
            }
        });
    });
}

function removeFriend(friendId) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: '/api/User/RemoveFriend?friendId=' + friendId,
            method: 'POST',
            success: function () {
                resolve(true);
            },
            error: function (error) {
                showError(error);
                reject(false);
            }
        });
    });
}

function getButtonHtml(user){
    if (user.hasFriendRequestPending) {
        return `<button id='cancel-${user.id}' onclick="callCancelFriendRequest('${user.id}')" class="cancel-btn btn">Cancel Follow Request</button>`;
    } else if (user.isFriend) {
        return `<button id='unfollow-${user.id}' title="Click to stop following" onClick="callRemoveFriend('${user.id}')" class="remove-friend-btn btn">Unfollow</button>`;
    } else {
        return `<button id='follow-${user.id}' onclick="callSendFriendRequest('${user.id}')" type="submit" class="send-request-btn btn">Follow</button>`;
    }
}

    //function getButtonHtml(sentFriendRequests, user) {
    //    let followRequestExists = false;
    //    let acceptedFriendExists = false;
//
//    for (const request of sentFriendRequests) {
//        if (request.receiverId === user.id) {
//            if (request.status === 'Pending') {
//                followRequestExists = true;
//            } else if (request.status === 'Accepted') {
//                acceptedFriendExists = true;
//            }
//        }
//
//        // If both conditions are true, we can break the loop early.
//        if (followRequestExists && acceptedFriendExists) {
//            break;
//        }
//    }
//
//    if (followRequestExists) {
//        return `<button onclick="callCancelFriendRequest('${user.id}')" class="cancel-btn">Cancel Follow Request</button>`;
//    } else if (acceptedFriendExists) {
//        return `<button title="Click to stop following" onClick="callRemoveFriend('${user.id}')" class="remove-friend-btn">Unfollow</button>`;
//    } else {
//        return `<button onclick="callSendFriendRequest('${user.id}')" type="submit" class="send-request-btn">Follow</button>`;
//    }
//}


function getButtonText(sentFriendRequests, user) {
    let followRequestExists = false;
    let acceptedFriendExists = false;

    for (const request of sentFriendRequests) {
        if (request.receiverId === user.id) {
            if (request.status === 'Pending') {
                followRequestExists = true;
            } else if (request.status === 'Accepted') {
                acceptedFriendExists = true;
            }
        }

        // If both conditions are true, we can break the loop early.
        if (followRequestExists && acceptedFriendExists) {
            break;
        }
    }

    if (followRequestExists) {
        return 'Cancel Follow Request';
    } else if (acceptedFriendExists) {
        return 'Unfollow';
    } else {
        return 'Follow';
    }
}


function getIconClass(user) {
    if (user.hasFriendRequestPending) {
        return 'yellow-icon';
    } else if (user.isFriend) {
        return 'red-icon';
    } else {
        return 'main-icon';
    }
}

//function getIconClass(sentFriendRequests, user) {
//    if (sentFriendRequests.some(i => i.receiverId === user.id && i.status === 'Pending')) {
//        return 'yellow-icon';
//    } else if (sentFriendRequests.some(i => i.receiverId === user.id && i.status === 'Accepted')) { 
//        return 'red-icon';
//    } else {
//        return 'main-icon';
//    }
//}


 async function setSocialCounts(followerElementId, followingElementId, postLikeElementId, currentUserId) {
    var followerElement = document.getElementById(followerElementId);
    var followingElement = document.getElementById(followingElementId);
    var postLikeElement = document.getElementById(postLikeElementId);

    var spinnerHtml = `
        <div class="spinner-border text-primary" role="status" style="color:var(--main-color) !important; width:1.1rem; height:1.1rem; ">
            <span class="sr-only"></span>
        </div>
    `;

    followerElement.innerHTML=  spinnerHtml;
    followingElement.innerHTML = spinnerHtml;
    postLikeElement.innerHTML = spinnerHtml;


    const followerCount = await getAllFollowersCount(currentUserId);
    followerElement.innerHTML = followerCount;
    
    const followingCount = await getAllFollowingsCount(currentUserId);
    followingElement.innerHTML = followingCount;
    
    const postLikeCount = await getAllPostLikeCount(currentUserId);
    postLikeElement.innerHTML = postLikeCount;
    //getAllFollowersCount(currentUserId)
    //.then(data => {
    //    alert(data);
    //    followerElement.innerHTML = data;
    //});
    //
    //    getAllFollowingsCount(currentUserId)
    //.then(data => {
    //    alert(data);
    //
    //    followingElement.innerHTML = data;
    //});
    //
    //    getAllPostLikeCount(currentUserId)
    //.then(data => {
    //    alert(data);
    //
    //    postLikeElement.innerHTML = data;
    //});
}

function makeAjaxRequest(url, type) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: url,
            type: type,
            success: function (response) {
                resolve(response);
            },
            error: function (error) {
                reject(error);
            }
        });
    });
}

// Neutral inline SVG avatar (gray silhouette). Used as a fallback when a profile photo is
// missing/undefined or fails to load. Inline so it can never 404 itself.
var DEFAULT_AVATAR =
    "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Crect width='100' height='100' fill='%23e2e6ea'/%3E%3Ccircle cx='50' cy='38' r='19' fill='%23adb5bd'/%3E%3Cpath d='M16 88c0-19 16-30 34-30s34 11 34 30z' fill='%23adb5bd'/%3E%3C/svg%3E";

// Swap any avatar image that fails to load (or has an undefined/empty src) for the default
// avatar. We only touch avatar-style images so broken covers/post media aren't replaced by a
// tiny silhouette. Capturing listener because `error` events don't bubble.
(function () {
    var AVATAR_CLASSES = ["rounded-circle", "user-image", "profile-image", "myimg"];

    function isAvatar(img) {
        for (var i = 0; i < AVATAR_CLASSES.length; i++) {
            if (img.classList && img.classList.contains(AVATAR_CLASSES[i])) {
                return true;
            }
        }
        return false;
    }

    document.addEventListener("error", function (e) {
        var img = e.target;
        if (!img || img.tagName !== "IMG" || img.dataset.fallbackApplied) {
            return;
        }
        if (isAvatar(img)) {
            img.dataset.fallbackApplied = "1";
            img.src = DEFAULT_AVATAR;
        }
    }, true);
})();

// Auto-trigger a "Load More" button when it scrolls into view, so users never have to click it.
// The button stays in the DOM (and keeps working if clicked); it's just fired automatically once
// it becomes visible and is enabled. Re-arms itself after each load.
function enableAutoLoad(buttonId) {
    var button = document.getElementById(buttonId);
    if (!button || !("IntersectionObserver" in window)) {
        return;
    }

    var isIntersecting = false;

    function tryLoad() {
        var visible = button.offsetParent !== null && getComputedStyle(button).visibility !== "hidden";
        if (isIntersecting && visible && !button.disabled) {
            button.click();
        }
    }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            isIntersecting = entry.isIntersecting;
        });
        tryLoad();
    }, { rootMargin: "200px" });

    observer.observe(button);

    // IntersectionObserver only fires when the intersection state *changes*. If the first
    // page of results isn't tall enough to push the button back out of view, it stays
    // continuously intersecting and would never auto-load again. The load handlers disable
    // the button (spinner) while loading and re-enable it when done, so watch the `disabled`
    // attribute and retry as soon as the button is enabled again while still in view.
    var enabledObserver = new MutationObserver(function () {
        tryLoad();
    });
    enabledObserver.observe(button, { attributes: true, attributeFilter: ["disabled"] });
}

// Stretches an element so its bottom reaches the bottom of the viewport, by setting a
// min-height equal to the space between the element's top and the viewport bottom. Used to make
// the white card on the notifications / friend-requests / chats pages fill the full available
// height even when it's empty, instead of shrinking to a small box. Re-applies on resize.
function fillToViewportHeight(el, bottomGap) {
    if (!el) {
        return;
    }
    var gap = bottomGap || 0;
    function apply() {
        var top = el.getBoundingClientRect().top;
        el.style.minHeight = (window.innerHeight - top - gap) + "px";
    }
    apply();
    window.addEventListener("resize", apply);
}

// Plays a short, pleasant chime for real-time events (new notification, friend request,
// incoming message). The tone is synthesized with the Web Audio API so there is no audio
// asset to ship and nothing can 404. Kept intentionally soft and brief so it's a gentle
// cue rather than an interruption.
//
// `type` tweaks the timbre:
//   "message" -> a single soft note (incoming chat message)
//   default   -> a two-note rising chime (notification / friend request)
var _zustAudioCtx = null;
var _zustLastSoundAt = 0;

// Lazily create (and return) the shared AudioContext.
function _zustGetAudioCtx() {
    var AudioCtx = window.AudioContext || window.webkitAudioContext;
    if (!AudioCtx) {
        return null; // Web Audio not supported
    }
    if (!_zustAudioCtx) {
        _zustAudioCtx = new AudioCtx();
    }
    return _zustAudioCtx;
}

// Browsers start an AudioContext "suspended" until the page receives a user gesture. Resume it
// on the first interaction so notification sounds are unlocked for the rest of the session,
// no matter which page the user is on when an event later arrives.
(function _zustUnlockAudioOnFirstGesture() {
    if (typeof document === "undefined") {
        return;
    }
    function unlock() {
        var ctx = _zustGetAudioCtx();
        if (ctx && ctx.state === "suspended" && ctx.resume) {
            ctx.resume();
        }
    }
    ["click", "keydown", "touchstart", "pointerdown"].forEach(function (evt) {
        // `once` removes the listener automatically after it fires the first time.
        document.addEventListener(evt, unlock, { once: true, passive: true });
    });
})();

function playNotificationSound(type) {
    try {
        // Throttle: several SignalR events can fire for one logical action (e.g. a friend
        // request sends ReceiveFriendRequest + ReceiveNotification). Collapse them into a
        // single chime instead of stacking sounds.
        var nowMs = Date.now();
        if (nowMs - _zustLastSoundAt < 500) {
            return;
        }
        _zustLastSoundAt = nowMs;

        var ctx = _zustGetAudioCtx();
        if (!ctx) {
            return; // Web Audio not supported; silently skip
        }

        // A context can be left "suspended" until the user interacts with the page. By the
        // time a real-time event arrives the user has usually interacted, so resuming here
        // unlocks playback; if not, the resume simply no-ops and the sound is skipped.
        if (ctx.state === "suspended" && ctx.resume) {
            ctx.resume();
        }

        // Master gain shared by the notes, with a quick attack and a smooth exponential
        // release so the chime never clicks or feels harsh.
        var now = ctx.currentTime;
        var master = ctx.createGain();
        master.gain.setValueAtTime(0.0001, now);
        master.gain.exponentialRampToValueAtTime(0.18, now + 0.02);
        master.gain.exponentialRampToValueAtTime(0.0001, now + 0.6);
        master.connect(ctx.destination);

        // Frequencies (Hz). A soft single note for messages; a gentle rising two-note
        // chime (A5 -> C#6) for notifications and friend requests.
        var notes = type === "message" ? [660] : [880, 1108.73];

        notes.forEach(function (freq, i) {
            var start = now + i * 0.13;
            var osc = ctx.createOscillator();
            osc.type = "sine";
            osc.frequency.value = freq;
            osc.connect(master);
            osc.start(start);
            osc.stop(start + 0.5);
        });
    } catch (e) {
        // Audio is a non-critical enhancement: never let it break the calling handler.
        console.debug("playNotificationSound skipped:", e);
    }
}