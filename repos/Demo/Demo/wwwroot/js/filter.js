$(document).ready(function () {
    // Event listener for the dropdown change event
    $("#appointmentStatus").on("change", function () {
        var selectedStatus = $(this).val(); // Get the selected value
        var doctorId = '@ViewBag.DoctorId';
        $.ajax({
            url: "/Management/Management/GetFilteredAppointments",
            type: "POST",
            data: { status: selectedStatus, doctorId: doctorId },
            success: function (response) {

                $("#stt").html(response);
            },
            error: function () {
                alert("An error occurred")
            }
    });
});
});