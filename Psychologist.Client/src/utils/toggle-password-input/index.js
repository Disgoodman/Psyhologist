const vTogglePassword = {
    mounted: (togglePasswordButton) => {
        let passwordInput = togglePasswordButton.parentElement.querySelector('input[type=password]');
        if (!passwordInput) return;
        passwordInput.classList.add("input-password");
        togglePasswordButton.classList.remove("d-none");
        togglePasswordButton.classList.add("toggle-password");

        console.log(1)
        togglePasswordButton.addEventListener('click', () => {
            passwordInput.type = passwordInput.type === "password" ? "text" : "password";
        });
    }
}

export default vTogglePassword;