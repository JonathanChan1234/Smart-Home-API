using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Services;
using smart_home_server.Mqtt.Client.Services;

namespace smart_home_server.Mqtt.Authentication;

public class MqttAuthenticationMiddlware : MqttAuthenticationBaseMiddlware
{
    private readonly ILogger<MqttAuthenticationMiddlware> _logger;
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IMqttClientService _mqttClientService;

    public MqttAuthenticationMiddlware(
        IAuthService authService,
        IJwtService jwtService,
        IMqttClientService mqttClientService,
        ILogger<MqttAuthenticationMiddlware> logger)
    {
        _logger = logger;
        _authService = authService;
        _jwtService = jwtService;
        _mqttClientService = mqttClientService;
    }

    public async override Task Handle()
    {
        try
        {
            var user = await _authService.GetUserByName(AuthenticationContext.MqttContext.UserName);
            if (user == null) throw new NullReferenceException("Non existing user");

            var validJWTPassword = await _jwtService.ValidateJwt(AuthenticationContext.MqttContext.Password, user.Id);
            if (!validJWTPassword) throw new BadAuthenticationException("Invalid JWT");

            var validMqttClient = await _mqttClientService.FindMqttClientById(int.Parse(AuthenticationContext.MqttContext.ClientId));
            if (validMqttClient == null) throw new NullReferenceException("Non existing mqtt client");
            if (validMqttClient.UserId != user.Id) throw new BadAuthenticationException("Invalid MQTT client ID");

            _logger.LogInformation($"Succeed to pass MQTT authentication middleware for user {user.Id}");
            await AuthSuccess();
        }
        catch (Exception e)
        {
            _logger.LogError($"Fail to pass MQTT authentication middleware, Error: {e.Message}");
            await AuthFailure();
        }
    }
}