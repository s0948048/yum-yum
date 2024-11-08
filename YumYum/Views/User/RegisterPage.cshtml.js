var Reg = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})*$/;
var Reg2=/^[\w]{6,}$/
$('#register-submit').on("click", function () {
    if (!Reg.test($('#register-email').val())) {
        alert("信箱不符合格式，請重新輸入")
    }
    else if (!Reg2.test($('#register-pwd').val())) {
        alert("密碼不符合格式，請重新輸入")
    }
    else if ($('#register-pwd').val() != $('#register-pwddb').val() ){
        alert("確認密碼與註冊密碼不同，請重新輸入")
    }
    else {
        window.location.href = 'User/LogInPage'
    }
})