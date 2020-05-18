(function () {
    "use strict";

    function autoGeoCodeController($scope, $q, AutoGeoCodeFactory) {

        var leafLetAccessToken = "pk.eyJ1IjoibWlndWVsbG9wZXo2IiwiYSI6ImNrOXd3a3NhZDA5eGkzZ284dXl5ZnI3NXUifQ.HehSLVuVEqpaa8ukk44NwA";
        var mapStarted = false;
        var myMap = null;
        var mapMarkers = [];

        $scope.mapId = "leaflet-map-" + $scope.id;

        function isMapElementVisible() {
            return angular.element("#" + $scope.mapId).is(":visible");
        }

        function attemptDrawMap() {
            if (!mapStarted) {
                drawMap().then(function () {
                        mapStarted = true;
                    },
                    function (e) {
                        console.log(e);
                    });
            } else {
                resetMap();
            }
        }

        function drawMap() {
            return $q(function (resolve, reject) {
                var latLon = $scope.model.value;
                if (latLon && latLon[0] !== null && latLon[0] !== undefined && latLon[1] !== null && latLon[1] !== undefined) {
                    try {
                        myMap = L.map($scope.mapId).setView(latLon, 13);
                        L.tileLayer("https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}", {
                            attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
                            maxZoom: 18,
                            id: "mapbox/streets-v11",
                            tileSize: 512,
                            zoomOffset: -1,
                            accessToken: leafLetAccessToken
                        }).addTo(myMap);
                        var marker = L.marker(latLon, { draggable: true }).addTo(myMap);
                        marker.on("dragend", function(r) {
                            // TODO: change saved values
                        });
                        mapMarkers = [marker];
                        return resolve();
                    } catch (e) {
                        return reject(e);
                    } 
                }
            });
        }

        function resetMap() {
            var latLon = $scope.model.value;

            if (latLon && latLon[0] !== null && latLon[0] !== undefined && latLon[1] !== null && latLon[1] !== undefined) {
                // clear markers
                for (var i = 0; i < mapMarkers.length; i++) {
                    myMap.removeLayer(mapMarkers[i]);
                }

                // recenter map and add new marker
                myMap.setView(latLon, 13);
                var marker = L.marker(latLon).addTo(myMap);
                mapMarkers = [marker];
            } 
        }

        $scope.displayAutoGeocode = function() {
            var lat = $scope.model.value[0];
            var lon = $scope.model.value[1];
            var latDisplay = lat !== null && lat !== undefined ? lat : "";
            var lonDisplay = lon !== null && lon !== undefined ? lon : "";

            return "[" + latDisplay + "," + lonDisplay + "]";
        };

        AutoGeoCodeFactory.initializeMaps()
            .then(function () {
                $scope.$watch(isMapElementVisible, function (newVal) {
                        if (newVal === true) {
                            attemptDrawMap();
                        }
                    });
                $scope.$watch("model.value",
                    function (newVal) {
                        if (isMapElementVisible()) {
                            attemptDrawMap();
                        }
                    });
            },
            function() {
                console.log("Failed to load map resources.");
            });
    }

    angular.module("umbraco").controller("AutoGeoCode.Controller", autoGeoCodeController);

})();