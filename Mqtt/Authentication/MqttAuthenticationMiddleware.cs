using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.Processors.Service;

namespace smart_home_server.Mqtt.Authentication;

public class MqttAuthenticationMiddlware : MqttAuthenticationBaseMiddlware
{
    private readonly ILogger<MqttAuthenticationMiddlware> _logger;
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IMqttClientService _mqttClientService;
    private readonly IProcessorService _processorService;

    public MqttAuthenticationMiddlware(
        IAuthService authService,
        IJwtService jwtService,
        IMqttClientService mqttClientService,
        IProcessorService processorService,
        ILogger<MqttAuthenticationMiddlware> logger)
    {
        _logger = logger;
        _authService = authService;
        _jwtService = jwtService;
        _mqttClientService = mqttClientService;
        _processorService = processorService;
    }

    public async override Task Handle()
    {
        try
        {
            int clientId = int.Parse(AuthenticationContext.MqttContext.ClientId);
            var username = AuthenticationContext.MqttContext.UserName;
            var password = AuthenticationContext.MqttContext.Password;

            // processor user
            var processor = await _processorService.GetProcessorByClientId(clientId);
            if (processor != null && password == processor.Password)
            {
                await AuthSuccess();
                return;
            }

            // normal user
            var user = await _authService.GetUserByName(username);
            if (user == null) throw new NullReferenceException("Non existing user");

            var validJWTPassword = await _jwtService.ValidateJwt(password, user.Id);
            if (!validJWTPassword) throw new BadAuthenticationException("Invalid JWT");

            var validMqttClient = await _mqttClientService.FindMqttClientById(clientId);
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