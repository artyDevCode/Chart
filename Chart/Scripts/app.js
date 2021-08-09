$(function () {
   
    $(".datepicker").datepicker({
        changeMonth: true,
       // altField: '#date_due',
     //   altFormat: 'yy-mm-dd',
        firstDay: 1, // rows starts on Monday 11/7/2014
        dateFormat: "dd-mm-yy",
        changeYear: true
       // timeFormat: "hh:mm tt"
    });

    /*.change(function () {
        //var aa = $('#EndDate').value;
        //var ab = $("#StartDate").value;
        if (this.id == 'StartDate') {
            if ((Date.parse(this.value) >= Date.parse(endDate)) {
                $("#EndDate").value = this.value;
            }
        }
    }); */
   
  //  $("#EndDate").value = data.endDate;
  //  $("#StartDate").value = data.startDate;
    
    $("#EndDate").change(function () {
        var startDate = $("#StartDate").value;
        var endDate = this.value;

        if ((Date.parse(endDate) < Date.parse(startDate))) {
            alert("Current End date should be greater than Start date");
            this.value = startDate;
        }
    });

    $("#StartDate").change(function () {
        var startDate = this.value;
        var endDate = document.getElementById("EndDate").value;

        if ((Date.parse(endDate) < Date.parse(startDate))) {
            alert("Current End date should be greater than Start date");
            this.value = endDate;
        }
    });

   // $("#StartDate").on("change", function () { var startdate = this.value; })
    // $("#EnddDte").on("change", function () { var enddate = this.value; })

    $("form.ajax").on("submit", function() {
        var that = $(this),
           url = that.attr('action'),
           type = that.attr('method'),
           data = {}; 

        that.find('[name]').each(function(index, value) {
            var that = $(this),
               name = that.attr('name'),
               value = that.val();
            data[name] = value;    
        });

        $.ajax({
            url: url,
            type : type,
            data : data,
            success: function(result) {
                var $target = $("#newCharts");
                var $newHtml = $(result);
                $target.replaceWith($newHtml);            
            }
        });
        return false;
    });
});