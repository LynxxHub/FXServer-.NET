function submitForm() {
    let name = document.getElementById('name').value;
    let age = document.getElementById('age').value;

    // Send data back to C# client script
    fetch(`https://lx_register/submitRegistrationForm`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset=UTF-8',
        },
        body: JSON.stringify({ name, age, nationality })
    });
}