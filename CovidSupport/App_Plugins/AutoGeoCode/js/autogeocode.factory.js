angular.module('umbraco.resources').factory('AutoGeoCodeFactory',

    function ($q) {

        var scriptUrl = "https://unpkg.com/leaflet@1.6.0/dist/leaflet.js";
        var styleUrl = "https://unpkg.com/leaflet@1.6.0/dist/leaflet.css";

        function scriptExists(url) {
            return document.querySelectorAll('script[src="' + url + '"]').length > 0;
        }
        function styleExists(url) {
            return document.querySelectorAll('link[rel=stylesheet][src="' + url + '"]').length > 0;
        }

        //Async loader
        var asyncLoad = function () {
            if (!styleExists(styleUrl)) {
                var style = document.createElement('link');
                style.setAttribute("rel", "stylesheet");
                style.setAttribute("href", styleUrl);
                document.head.appendChild(style);
            }
            if (!scriptExists(scriptUrl)) {
                var script = document.createElement('script');
                script.src = scriptUrl;
                document.head.appendChild(script);
            }
        };

        return {
            mapsInitialized: function () {
                var deferred = $q.defer();
                asyncLoad();
                deferred.resolve();
                return deferred.promise;
            }
        };
    }
);