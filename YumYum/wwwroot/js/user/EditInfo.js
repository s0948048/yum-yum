
window.addEventListener('load', function () {

	function updateCount() {
		var cs = $(this).val().length;
		if (this.id == "input-intro") {
			$('#text-intro').text(`${cs}/200`);
		}
		else if (this.id == "input-nickname") {
			$('#text-nickname').text(`${cs}/16`);
		}
	}

	$('textarea').keyup(updateCount);
	$('textarea').keydown(updateCount);
	$('input').keyup(updateCount);
	$('input').keydown(updateCount);

	$("#close-instagram").on('click', () => {
		document.getElementById("instagram-edit").click();
	})
	$("#close-facebook").on('click', () => {
		document.getElementById("facebook-edit").click();
	})
	$("#close-youtube").on('click', () => {
		document.getElementById("youtube-edit").click();
	})
	$("#close-create").on('click', () => {
		document.getElementById("create-edit").click();
	})


})
