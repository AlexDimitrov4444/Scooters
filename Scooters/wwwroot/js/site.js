document.addEventListener("submit", function (event) {
    var form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.classList.contains("js-confirm-delete")) {
        return;
    }

    var message = form.getAttribute("data-confirm") || "Изтриване?";
    if (!window.confirm(message)) {
        event.preventDefault();
    }
});

document.querySelectorAll(".js-auto-submit").forEach(function (input) {
    input.addEventListener("change", function () {
        var form = input.closest("form");
        if (form) {
            form.submit();
        }
    });
});

document.querySelectorAll(".js-image-preview").forEach(function (input) {
    input.addEventListener("change", function () {
        var selector = input.getAttribute("data-preview");
        var preview = selector ? document.querySelector(selector) : null;
        if (!(preview instanceof HTMLImageElement) || !input.files || !input.files[0]) {
            return;
        }

        preview.src = URL.createObjectURL(input.files[0]);
        preview.classList.remove("d-none");
    });
});
