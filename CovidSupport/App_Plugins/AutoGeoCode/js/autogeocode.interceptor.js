angular.module('umbraco.services').config([
    '$httpProvider',
    function ($httpProvider, $rootScope) {
        $httpProvider.interceptors.push(function ($q) {
            return {
                'response': function (response) {
                    //console.log(response.config.url);
                    // Intercept requests content editor data responses
                    if (response.config.url.indexOf("/umbraco/backoffice/UmbracoApi/Content/GetById") === 0) {
                        console.log(response);
                        angular.forEach(response.data.variants,
                            function(variant) {
                                angular.forEach(variant.tabs,
                                    function(tab) {
                                        angular.forEach(tab.properties,
                                            function(property) {
                                                if (property.alias === "lat" || property.alias === "lon") {
                                                    // TODO: is there a way to watch this?
                                                    //console.log(property);
                                                }
                                            });
                                    });
                            });
                    }

                    return response;
                }
            };
        });

    }]);