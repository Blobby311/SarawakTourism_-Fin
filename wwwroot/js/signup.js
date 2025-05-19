document.getElementById('signupForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    const email = document.getElementById('email').value;
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, username, password })
    });

    const result = await response.json();
    if (response.ok) {
        alert("Signup successful! Please log in.");
        window.location.href = "login.html";
    } else {
        alert(result.message || "Signup failed");
    }
});