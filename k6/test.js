import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  // A number specifying the number of VUs to run concurrently.
  vus: 100,
  // A string specifying the total duration of the test run.
  duration: '600s',

  // The following section contains configuration options for execution of this
  // test script in Grafana Cloud.
  //
  // See https://grafana.com/docs/grafana-cloud/k6/get-started/run-cloud-tests-from-the-cli/
  // to learn about authoring and running k6 test scripts in Grafana k6 Cloud.
  //
  cloud: {
    // The ID of the project to which the test is assigned in the k6 Cloud UI.
    // By default tests are executed in default project.
    projectID: 3718133,
    // The name of the test in the k6 Cloud UI.
    // Test runs with the same name will be grouped.
    name: "test.js"
  },
};

// The function that defines VU logic.
//
// See https://grafana.com/docs/k6/latest/examples/get-started-with-k6/ to learn more
// about authoring k6 scripts.
//
export default function() {
  http.get('https://eshop-public-api-axcehxeecpe4hzds.northeurope-01.azurewebsites.net/api/catalog-items');
  sleep(0.1);
}
