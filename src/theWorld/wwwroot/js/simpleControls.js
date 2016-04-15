(function() {
    'use strict';

    angular.module('simpleControls', [])
        .directive('waitCursor', waitCursor);

    function waitCursor() {
        return {
            scope: {
                show: '=displayWhen'
            },
            restrict: 'EA',
            templateUrl: '/views/waitCursor.html'
    };
    }
}())