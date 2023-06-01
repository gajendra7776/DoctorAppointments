
    // Add event listener to the submit button
$(document).ready(function () {
    $('#Createform').submit(function (event) {
        var data = 1;
        var patientName = document.getElementById("patientName").value;
        if (patientName.toUpperCase() === "BOB") {
            data = 0;
            alert("Patient name cannot be 'BOB'. Please enter a different name.");
        }
        debugger
        // Get the selected appointment date
        var selectedDate = new Date($('#appointmentDate').val());

        // Get the current date
        var currentDate = new Date();
        var flag = 1;
        // Compare the selected date with the current date
        if (selectedDate < currentDate) {
            flag = 0;
            // Appointment date is not valid
            alert('Please select a date greater than today!');
        }
        if (flag == 0 || data == 0) {

            event.preventDefault();
        }
       
    });
});

 
