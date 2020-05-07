(function () {
    'use strict';

    function autoGeoCodeController($scope, AutoGeoCodeFactory) {

        $scope.mapId = "leaflet-map-" + $scope.$id;

        function getLatLon() {
            var content = $scope.$parent.$parent.$parent.content;

            angular.forEach(content.tabs,
                function(tab) {
                    angular.forEach(tab.properties,
                        function(property) {
                            if (property.alias === "lat") {
                                $scope.lat = property.value;
                            }
                            if (property.alias === "lon") {
                                $scope.lon = property.value;
                            }
                        });
                });

            console.log($scope.lat + ", " + $scope.lon);
        }

        function initMap() {
            AutoGeoCodeFactory.mapsInitialized().then(function () {
                
                var mymap = L.map($scope.mapId).setView([35.929903, -79.026481], 13);
                
                L.tileLayer('https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}', {
                    attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
                    maxZoom: 18,
                    id: 'mapbox/streets-v11',
                    tileSize: 512,
                    zoomOffset: -1,
                    accessToken: 'pk.eyJ1IjoibWlndWVsbG9wZXo2IiwiYSI6ImNrOXd3a3NhZDA5eGkzZ284dXl5ZnI3NXUifQ.HehSLVuVEqpaa8ukk44NwA'
                }).addTo(mymap);
                var marker = L.marker([35.929903, -79.026481]).addTo(mymap);
            });
        }

        initMap();
    }

    angular.module('umbraco').controller("AutoGeoCode.Controller", autoGeoCodeController);

})();