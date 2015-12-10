var app = angular.module('jitter', []);
app.controller('TestController', ['$scope', "$http", function ($scope, $http) {
    $scope.test = "test variable";
    $scope.hello = function () {
        $scope.test = "Hello World";

        $http.get("/api/Test")
            .success(function (data) {
                $scope.test = data;})
            .error(function (error) { alert(error.error) });
    }
}]);