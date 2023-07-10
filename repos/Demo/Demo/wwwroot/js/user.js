$(document).ready(function () {

        $.ajax({
            url: '/User/User/GetHospitals',
            success: function (response) {
                var hospitalSelect = $('#hospitalSelect');
                hospitalSelect.empty();
                hospitalSelect.append('<option value="">Select Hospital</option>');
                $.each(response, function (index, hospital) {
                    hospitalSelect.append('<option value="' + hospital.hospitalId + '">' + hospital.hospitalName + '</option>');
                });
            }
        });

    $('#hospitalSelect').on('change', function () {
        var doctorSelect = $('#doctorSelect');
        doctorSelect.empty();
        var selectedHospitalId = $(this).val();
        $.ajax({
            url: '/User/User/GetDoctorTypes',
            data: { hospitalId: selectedHospitalId },
            success: function (response) {
                
                var doctorTypeSelect = $('#doctorTypeSelect');
                doctorTypeSelect.empty();
                doctorTypeSelect.append('<option value="">Select Doctor Type</option>');
                $.each(response, function (index, doctorType) {
                    doctorTypeSelect.append('<option value="' + doctorType.doctorTypeId + '">' + doctorType.doctorTypeName + '</option>');
                });
            }
        });
    });

    $('#doctorTypeSelect').on('change', function () {
        
        var selectedDoctorTypeId = $(this).val();
        var selectedHospitalId = $('#hospitalSelect').val();
        $.ajax({
            url: '/User/User/GetDoctors',
            data: { doctorTypeId: selectedDoctorTypeId, hospitalId: selectedHospitalId},
            success: function (response) {
                var doctorSelect = $('#doctorSelect');
                doctorSelect.empty();
                doctorSelect.append('<option value="">Select Doctor</option>');
                $.each(response, function (index, doctor) {
                    doctorSelect.append('<option value="' + doctor.doctorID + '">' + doctor.doctorName + '</option>');
                });
            }
        });
    });

    $.ajax({
        url: '/User/User/GetUsers',
        success: function (response) {
            var userSelect = $('#userSelect');
            userSelect.empty();
            userSelect.append('<option value="">Select User</option>');
            $.each(response, function (index, user) {
                userSelect.append('<option value="' + user.userId + '">' + user.userName + '</option>');
            });
        }
    });

});

