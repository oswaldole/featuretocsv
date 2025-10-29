@regression @authentication
Feature: User Login
  As a user
  I want to log into the system
  So that I can access my account

  @critical @smoke
  Scenario: Successful login with valid credentials
    Given the user is on the login page
    When the user enters username "testuser@example.com"
    And the user enters password "SecurePass123"
    And the user clicks the login button
    Then the user should be redirected to the dashboard
    And the user should see a welcome message

  @high
  Scenario: Failed login with invalid password
    Given the user is on the login page
    When the user enters username "testuser@example.com"
    And the user enters password "WrongPassword"
    And the user clicks the login button
    Then the user should see an error message "Invalid credentials"
    And the user should remain on the login page

  @medium
  Scenario: Login with empty credentials
    Given the user is on the login page
    When the user clicks the login button
    Then the user should see validation errors
      """
      Username is required
      Password is required
      """

  Scenario Outline: Login with different user roles
    Given the user is on the login page
    When the user enters username "<username>"
    And the user enters password "<password>"
    And the user clicks the login button
    Then the user should be redirected to the "<dashboard_type>" dashboard
    And the user role should be "<role>"

    Examples:
      | username           | password    | dashboard_type | role  |
      | admin@example.com  | AdminPass1  | admin          | Admin |
      | user@example.com   | UserPass1   | user           | User  |
      | guest@example.com  | GuestPass1  | guest          | Guest |
