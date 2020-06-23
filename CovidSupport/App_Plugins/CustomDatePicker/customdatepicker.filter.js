angular.module('umbraco.filters').filter('customTimeAmPmFormat', function () {
    return function (value) {
        var d = new Date(value);

        if (d instanceof Date && !isNaN(d)) {
            var hour = d.getHours();
            var minute = d.getMinutes();
            var t;

            if (hour === 0) {
                hour = 12;
                t = "AM";
            } else if (hour > 0 && hour < 12) {
                t = "AM";
            } else if (hour === 12) {
                t = "PM";
            } else {
                hour = hour - 12;
                t = "PM";
            }

            var hourVal = hour < 10 ? "0" + hour : "" + hour;
            var minuteVal = minute < 10 ? "0" + minute : "" + minute;

            return hourVal + ":" + minuteVal + " " + t;
        } else {
            return null;
        }
    };
});
