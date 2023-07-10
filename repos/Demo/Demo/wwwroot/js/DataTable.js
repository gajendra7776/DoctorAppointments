$(document).ready(function () {
     Initialize DataTable
    var table = $('#MyDataTable').DataTable();
















  //   $.ajax({
  //    url: '/User/GetHospitals', 
  //    success: function(response) {
          
  //    var hospitalSelect = $('#hospitalSelect');
  //    hospitalSelect.empty();
  //        hospitalSelect.append('<option value="">Select Hospital</option>');
  //        console.log(response)
  //        $.each(response, function (index, hospital) {
             
  //            hospitalSelect.append('<option value="' + hospital.hospitalId + '">' + hospital.hospitalName + '</option>');
  //    });
  //  }
  //});

  //  $.ajax({
  //      url: '/User/GetDoctorTypes',
  //      success: function (response) {
  //          console.log(response)
  //          var doctorTypeSelect = $('#doctorTypeSelect');
  //          doctorTypeSelect.empty();
  //          doctorTypeSelect.append('<option value="">Select DoctorType</option>');
  //          $.each(response, function (index, doctorTypeSelects) {
  //              doctorTypeSelect.append('<option value="' + doctorTypeSelects.doctorTypeId + '">' + doctorTypeSelects.doctorType + '</option>');
  //          });
  //      }
  //  }); 

    




});


function filterDoctors() {
    var hospitalId = $('#hospitalSelect').val();
    var doctorTypeId = $('#doctorTypeSelect').val();
    var status = $('#status').val();

    // Send AJAX request to update filtered results
    $.ajax({
        url: "/Management/DisplayDoctor", // Replace with the actual URL and action method
        type: "POST",
        data: {
            hospitalID: hospitalId,
            doctorTypeID: doctorTypeId,
            Status: status
        },
        success: function (response) {
            console.log(response)
        //    $('.Userpaged').html($(response).find('.Userpaged').html());

            $('#MyDataTable').html($(response).find('#MyDataTable').html());
           
        },
        error: function () {
            alert('Error');
        }
    });
}

// Attach change event listener to select elements
$('#hospitalSelect, #doctorTypeSelect, #status').change(filterDoctors);