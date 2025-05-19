document.addEventListener('DOMContentLoaded', () => {
    // --- Get DOM Elements ---
    const createPostBtn = document.getElementById('createPostBtn');
    const notificationsBtn = document.getElementById('notificationsBtn');
    const createPostModal = document.getElementById('createPostModal');
    const closeModalBtn = document.getElementById('closeModalBtn');
    const submitPostBtn = document.getElementById('submitPostBtn');
    const postFeed = document.getElementById('postFeed');
    const notificationsDropdown = document.getElementById('notificationsDropdown');
    const notificationCountBadge = document.getElementById('notification-count');
    const logoutBtn = document.getElementById('logoutBtn');

    // Initialize notifications dropdown styling
    if (notificationsDropdown) {
        notificationsDropdown.style.display = 'none';
        notificationsDropdown.style.position = 'absolute';
        notificationsDropdown.style.top = '100%';
        notificationsDropdown.style.right = '0';
        notificationsDropdown.style.width = '300px';
        notificationsDropdown.style.maxHeight = '400px';
        notificationsDropdown.style.overflowY = 'auto';
        notificationsDropdown.style.backgroundColor = '#fff';
        notificationsDropdown.style.boxShadow = '0 2px 5px rgba(0,0,0,0.2)';
        notificationsDropdown.style.borderRadius = '4px';
        notificationsDropdown.style.zIndex = '1000';
    }

    // Initialize notification count badge styling
    if (notificationCountBadge) {
        notificationCountBadge.style.display = 'none';
        notificationCountBadge.style.position = 'absolute';
        notificationCountBadge.style.top = '-5px';
        notificationCountBadge.style.right = '-5px';
        notificationCountBadge.style.backgroundColor = '#ff4444';
        notificationCountBadge.style.color = '#fff';
        notificationCountBadge.style.borderRadius = '50%';
        notificationCountBadge.style.padding = '2px 6px';
        notificationCountBadge.style.fontSize = '12px';
        notificationCountBadge.style.minWidth = '18px';
        notificationCountBadge.style.textAlign = 'center';
    }

    // --- Logout Button ---
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            console.log('Logout clicked');
            localStorage.removeItem('username');
            window.location.href = 'homepage.html'; // Redirect to login or homepage
        });
    }

    // --- Fetch and Display Posts ---
    async function loadPosts() {
        try {
            const response = await fetch('/api/posts');
            const responseText = await response.text();
            console.log('Raw posts response:', responseText);

            if (!responseText.trim()) {
                console.error('Empty response received for posts');
                return;
            }

            let result;
            try {
                result = JSON.parse(responseText);
                console.log('Successfully parsed posts response:', result);
            } catch (parseError) {
                console.error('Failed to parse posts response:', parseError);
                return;
            }

            postFeed.innerHTML = '<h2>SwkThoughts</h2>'; // Keep heading
            if (result.success && result.data) {
                // Handle both array and $values format
                const posts = Array.isArray(result.data) ? result.data : 
                            (result.data.$values || []);
                
                posts.forEach(post => {
                    const postElement = createPostElement(
                        post.title,
                        post.content,
                        post.username,
                        post.createdAt,
                        post.id,
                        post.likes // Pass likes to the post element
                    );
                    postFeed.appendChild(postElement);

                    // Fetch and display comments for this post
                    const commentList = postElement.querySelector('.comment-list');
                    loadComments(post.id, commentList);
                });
            } else {
                console.error('Failed to load posts:', result.message || 'Unknown error');
            }
        } catch (error) {
            console.error('Error loading posts:', error);
        }
    }
    loadPosts();

    // --- Fetch and Display Comments ---
    async function loadComments(postId, commentListDiv) {
        try {
            const response = await fetch(`/api/comments/post/${postId}`);
            console.log('Comments response status:', response.status);
            console.log('Comments response headers:', Object.fromEntries(response.headers.entries()));

            const responseText = await response.text();
            console.log('Raw comments response:', responseText);

            if (!responseText.trim()) {
                console.log('Empty response received for comments');
                commentListDiv.innerHTML = '';
                return;
            }

            let result;
            try {
                result = JSON.parse(responseText);
                console.log('Successfully parsed comments response:', result);
            } catch (parseError) {
                console.error('Failed to parse comments response:', parseError);
                console.error('Raw response that failed to parse:', responseText);
                return;
            }

            if (result.success && result.data) {
                // Handle both array and $values format
                const comments = Array.isArray(result.data) ? result.data : 
                               (result.data.$values || []);
                
                commentListDiv.innerHTML = ''; // Clear existing comments
                comments.forEach(comment => {
                    const commentDiv = document.createElement('div');
                    commentDiv.classList.add('comment');
                    commentDiv.innerHTML = `<strong>${escapeHTML(comment.username)}:</strong> ${escapeHTML(comment.content)}`;
                    commentListDiv.appendChild(commentDiv);
                });
            } else {
                console.error('Failed to load comments:', result.message || 'Unknown error');
            }
        } catch (error) {
            console.error('Error loading comments:', error);
        }
    }

    // --- Fetch and Display Notifications ---
    async function loadNotifications() {
        const username = localStorage.getItem('username');
        if (!username) {
            console.log('No username found in localStorage, skipping notification load');
            return;
        }

        // Don't reload if dropdown is open
        if (notificationsDropdown && notificationsDropdown.style.display === 'block') {
            return;
        }

        console.log('Loading notifications for user:', username);

        try {
            const response = await fetch(`/api/notifications?username=${encodeURIComponent(username)}`);
            console.log('Notification response status:', response.status);
            
            const responseText = await response.text();
            console.log('Raw notification response:', responseText);

            // Check if response is empty
            if (!responseText.trim()) {
                console.log('Empty response received, initializing empty notifications');
                updateNotificationsUI([]);
                return;
            }

            let result;
            try {
                result = JSON.parse(responseText);
                console.log('Successfully parsed notification response:', result);
            } catch (parseError) {
                console.error('Failed to parse notification response:', parseError);
                console.error('Raw response that failed to parse:', responseText);
                updateNotificationsUI([]);
                return;
            }

            if (result.success && result.data) {
                // Handle both array and $values format
                const notifications = Array.isArray(result.data) ? result.data : 
                                   (result.data.$values || []);
                
                if (notifications.length === 0) {
                    console.log('No notifications found for user');
                } else {
                    console.log(`Found ${notifications.length} notifications for user:`, notifications);
                }
                updateNotificationsUI(notifications);
            } else {
                console.error('Invalid notification response format:', result);
                updateNotificationsUI([]);
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
            updateNotificationsUI([]);
        }
    }

    // Helper function to update the notifications UI
    function updateNotificationsUI(notifications) {
        if (!notificationsDropdown) {
            console.log('Notifications dropdown element not found');
            return;
        }

        console.log('Updating notifications UI with:', notifications);
        notificationsDropdown.innerHTML = ''; // Clear existing notifications

        if (notifications.length === 0) {
            console.log('Displaying empty notifications state');
            const noNotifications = document.createElement('p');
            noNotifications.textContent = 'No new notifications.';
            noNotifications.style.padding = '10px';
            noNotifications.style.textAlign = 'center';
            noNotifications.style.color = '#666';
            notificationsDropdown.appendChild(noNotifications);
        } else {
            console.log(`Displaying ${notifications.length} notifications`);
            notifications.forEach(notification => {
                console.log('Adding notification to UI:', notification);
                const notificationItem = document.createElement('div');
                notificationItem.style.padding = '10px';
                notificationItem.style.borderBottom = '1px solid #eee';
                notificationItem.style.margin = '0';
                notificationItem.style.cursor = 'pointer';
                notificationItem.style.backgroundColor = '#fff';
                notificationItem.style.transition = 'background-color 0.2s';
                
                notificationItem.addEventListener('mouseover', () => {
                    notificationItem.style.backgroundColor = '#f5f5f5';
                });
                
                notificationItem.addEventListener('mouseout', () => {
                    notificationItem.style.backgroundColor = '#fff';
                });
                
                const message = document.createElement('p');
                message.textContent = notification.message;
                message.style.margin = '0';
                message.style.color = '#333';
                
                const time = document.createElement('small');
                time.textContent = new Date(notification.createdAt).toLocaleString();
                time.style.color = '#666';
                time.style.fontSize = '0.8em';
                
                notificationItem.appendChild(message);
                notificationItem.appendChild(time);
                notificationsDropdown.appendChild(notificationItem);
            });
        }

        // Update notification count badge if it exists
        if (notificationCountBadge) {
            console.log('Updating notification count badge to:', notifications.length);
            notificationCountBadge.textContent = notifications.length;
            notificationCountBadge.style.display = notifications.length > 0 ? 'inline' : 'none';
        }
    }

    // Load notifications every 30 seconds
    let notificationInterval = setInterval(loadNotifications, 30000);

    // Initial load
    loadNotifications();

    // Clean up interval when page is unloaded
    window.addEventListener('beforeunload', () => {
        clearInterval(notificationInterval);
    });

    // --- Open Create Post Modal ---
    if (createPostBtn) {
        createPostBtn.addEventListener('click', () => {
            createPostModal.style.display = 'flex';
        });
    }

    // --- Close Create Post Modal ---
    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', () => {
            createPostModal.style.display = 'none';
        });
    }
    if (createPostModal) {
        createPostModal.addEventListener('click', (event) => {
            if (event.target === createPostModal) {
                createPostModal.style.display = 'none';
            }
        });
    }

    // --- Submit New Post ---
    if (submitPostBtn) {
        submitPostBtn.addEventListener('click', async () => {
            const titleInput = document.getElementById('postTitleInput');
            const contentInput = document.getElementById('postContentInput');
            const title = titleInput.value.trim();
            const content = contentInput.value.trim();
            const username = localStorage.getItem('username');
            if (!username) {
                alert('You must be logged in to post.');
                return;
            }
            if (title && content) {
                try {
                    const response = await fetch('/api/posts', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ title, content, username })
                    });
                    const result = await response.json();
                    if (result.success) {
                        loadPosts();
                        createPostModal.style.display = 'none';
                        titleInput.value = '';
                        contentInput.value = '';
                    } else {
                        alert(result.message || "Failed to create post.");
                    }
                } catch (error) {
                    console.error('Error creating post:', error);
                }
            } else {
                alert('Please fill in both the title and content.');
            }
        });
    }

    // --- Like & Comment Button Handling ---
    if (postFeed) {
        postFeed.addEventListener('click', (event) => {
            // Like Button
            if (event.target.classList.contains('like-button')) {
                handleLikeButtonClick(event.target);
            }
            // Comment Button
            if (event.target.classList.contains('comment-button')) {
                const postElement = event.target.closest('.post');
                if (postElement) {
                    const commentsSection = postElement.querySelector('.comments-section');
                    if (commentsSection) {
                        commentsSection.style.display = commentsSection.style.display === 'none' ? 'block' : 'none';
                    }
                }
            }
            // Submit Comment Button
            if (event.target.classList.contains('submit-comment-button')) {
                handleCommentSubmit(event);
            }
        });
    }

    // --- Notifications Dropdown ---
    if (notificationsBtn) {
        notificationsBtn.addEventListener('click', async (event) => {
            event.stopPropagation();
            const isDisplayed = notificationsDropdown.style.display === 'block';
            notificationsDropdown.style.display = isDisplayed ? 'none' : 'block';

            if (!isDisplayed) {
                try {
                    const username = localStorage.getItem('username');
                    if (!username) {
                        console.log('No username found in localStorage');
                        return;
                    }

                    // First load current notifications
                    const response = await fetch(`/api/notifications?username=${encodeURIComponent(username)}`);
                    const responseText = await response.text();
                    let result;
                    try {
                        result = JSON.parse(responseText);
                    } catch (parseError) {
                        console.error('Failed to parse notifications:', parseError);
                        return;
                    }

                    const notifications = result.success && result.data ? 
                        (Array.isArray(result.data) ? result.data : result.data.$values || []) : [];

                    // Only mark as read if there are notifications
                    if (notifications.length > 0) {
                        console.log('Marking notifications as read for user:', username);
                        const markReadResponse = await fetch(`/api/notifications/mark-read?username=${encodeURIComponent(username)}`, { 
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' }
                        });

                        if (markReadResponse.ok) {
                            console.log('Successfully marked notifications as read');
                            // Update the UI to show notifications as read
                            updateNotificationsUI(notifications);
                            // Reset notification count
                            if (notificationCountBadge) {
                                notificationCountBadge.textContent = '0';
                                notificationCountBadge.style.display = 'none';
                            }
                        } else {
                            console.error('Failed to mark notifications as read');
                        }
                    }
                } catch (error) {
                    console.error('Error handling notifications:', error);
                }
            }
        });
    }
    document.addEventListener('click', (event) => {
        if (notificationsDropdown && notificationsDropdown.style.display === 'block') {
            if (!notificationsBtn.contains(event.target) && !notificationsDropdown.contains(event.target)) {
                notificationsDropdown.style.display = 'none';
            }
        }
    });

    // --- Helper Functions ---

    async function handleLikeButtonClick(button) {
        const postElement = button.closest('.post');
        const postId = postElement.dataset.postId;
        const username = localStorage.getItem('username');

        if (!username) {
            alert('You must be logged in to like posts.');
            return;
        }

        try {
            // Ensure we have a valid postId
            if (!postId) {
                console.error('Invalid post ID');
                return;
            }

            console.log('Sending like request for post:', postId, 'from user:', username);

            const response = await fetch(`/api/posts/${postId}/like?username=${encodeURIComponent(username)}`, { 
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            });

            console.log('Like response status:', response.status);
            console.log('Like response headers:', Object.fromEntries(response.headers.entries()));

            const responseText = await response.text();
            console.log('Raw like response:', responseText);

            if (!responseText.trim()) {
                console.error('Empty response received');
                return;
            }

            let result;
            try {
                result = JSON.parse(responseText);
                console.log('Parsed like response:', result);
            } catch (parseError) {
                console.error('Failed to parse like response:', parseError);
                console.error('Raw response:', responseText);
                return;
            }

            if (result.success) {
                console.log('Like successful, new like count:', result.data?.likes);
                button.textContent = `Like (${result.data?.likes ?? 0})`;
                // Reload notifications to show the new like notification
                await loadNotifications();
            } else {
                console.error('Like failed:', result.message);
                alert(result.message || 'Failed to like the post. Please try again.');
            }
        } catch (error) {
            console.error('Error liking post:', error);
            alert('Failed to like the post. Please try again.');
        }
    }

    async function handleCommentSubmit(event) {
        event.preventDefault();
        console.log('Starting comment submission...');

        // Get the submit button and traverse up to find the add-comment div
        const submitButton = event.target;
        const addCommentDiv = submitButton.closest('.add-comment');
        
        if (!addCommentDiv) {
            console.error('Add comment div not found');
            alert('Error: Comment form not found');
            return;
        }

        // Get the post element to get the post ID
        const postElement = addCommentDiv.closest('.post');
        if (!postElement) {
            console.error('Post element not found');
            alert('Error: Post not found');
            return;
        }

        const postId = postElement.dataset.postId;
        const commentInput = addCommentDiv.querySelector('.comment-input');
        
        if (!commentInput) {
            console.error('Comment input element not found');
            alert('Error: Comment input field not found');
            return;
        }

        const commentText = commentInput.value.trim();
        const username = localStorage.getItem('username');

        console.log('Comment data:', { postId, commentText, username });

        if (!postId) {
            console.error('No post ID found');
            alert('Error: Post ID is missing');
            return;
        }

        if (!commentText) {
            console.error('Comment text is empty');
            alert('Please enter a comment');
            return;
        }

        if (!username) {
            console.error('Username not found');
            alert('Please log in to comment');
            return;
        }

        try {
            // Create a FormData object to send the data
            const formData = new FormData();
            formData.append('Content', commentText);
            formData.append('Username', username);
            formData.append('PostId', postId);

            console.log('Sending comment data:', {
                Content: commentText,
                Username: username,
                PostId: postId
            });

            const response = await fetch('/api/comments', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    Content: commentText,
                    Username: username,
                    PostId: parseInt(postId)
                })
            });

            console.log('Raw response:', response);
            const responseText = await response.text();
            console.log('Raw response text:', responseText);

            if (!response.ok) {
                let errorMessage = 'Failed to submit comment';
                try {
                    const errorData = JSON.parse(responseText);
                    console.error('Error data:', errorData);
                    
                    // Handle validation errors
                    if (errorData.errors) {
                        const validationErrors = Object.entries(errorData.errors)
                            .map(([field, errors]) => {
                                // Map field names to user-friendly names
                                const fieldMap = {
                                    'Content': 'Comment text',
                                    'Username': 'Username',
                                    'PostId': 'Post ID'
                                };
                                const friendlyName = fieldMap[field] || field;
                                return `${friendlyName}: ${errors.join(', ')}`;
                            })
                            .join('\n');
                        errorMessage = `Please fix the following errors:\n${validationErrors}`;
                    } else if (errorData.message) {
                        errorMessage = errorData.message;
                    }
                } catch (e) {
                    console.error('Error parsing error response:', e);
                }
                throw new Error(errorMessage);
            }

            const result = JSON.parse(responseText);
            console.log('Comment submission result:', result);

            if (result.success) {
                // Clear the form
                commentInput.value = '';
                
                // Refresh the comments section
                const commentList = postElement.querySelector('.comment-list');
                await loadComments(postId, commentList);
                
                // Show success message
                alert('Comment added successfully!');
            } else {
                throw new Error(result.message || 'Unknown error');
            }
        } catch (error) {
            console.error('Error submitting comment:', error);
            alert(error.message || 'Failed to submit comment. Please try again.');
        }
    }

    function createPostElement(title, content, author, createdAt, id, likes = 0) {
        const article = document.createElement('article');
        article.classList.add('post');
        article.dataset.postId = id || Date.now();
        article.innerHTML = `
            <h3>${escapeHTML(title)}</h3>
            <p class="post-meta">Posted by ${escapeHTML(author)} on <time>${createdAt ? new Date(createdAt).toLocaleDateString() : new Date().toLocaleDateString()}</time></p>
            <p class="post-content">${escapeHTML(content)}</p>
            <div class="post-actions">
                <button class="like-button">Like (${likes})</button>
                <button class="comment-button">Comment</button>
            </div>
            <div class="comments-section" style="display: none;">
                <h4>Comments</h4>
                <div class="comment-list"></div>
                <div class="add-comment">
                    <input type="text" class="comment-input" placeholder="Write a comment...">
                    <button class="submit-comment-button">Post Comment</button>
                </div>
            </div>
        `;
        return article;
    }

    function escapeHTML(str) {
        const div = document.createElement('div');
        div.appendChild(document.createTextNode(str));
        return div.innerHTML;
    }
});