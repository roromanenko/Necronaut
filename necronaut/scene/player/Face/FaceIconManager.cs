using Godot;
using System;

public partial class FaceIconManager : Node2D
{
	private Node2D _current; // Текущий отображаемый объект

	private Node2D _fullHP;
	private Node2D _lowDamage;
	private Node2D _halfHP;
	private Node2D _lowHP;
	private CharacterBody2D _player;
	private AnimatedSprite2D _animatedSprite;

	private int _maxHp = 0; // Максимальное HP игрока
	private int _curHp = 0;

	private int PlayerCurrentHp => _player.Get("healPoints").AsInt32();

	public void SwitchFaceIcon(double hpPercentage)
	{
		// Логируем переключение иконки
		GD.Print($"Switching face icon to hpPercentage: {hpPercentage}");

		// Скрываем текущий объект
		if (_current != null)
		{
			_current.Visible = false;
			GD.Print($"Hiding current face icon: {_current.Name}");
		}

		_current = hpPercentage switch
		{
			>= 0.75 => _fullHP,
			>= 0.5 and < 0.75 => _lowDamage,
			>= 0.25 and < 0.5 => _halfHP,
			< 0.25 => _lowHP,
			_ => throw new ArgumentException("Invalid condition for face icon!")
		};

		// Показываем новый объект
		if (_current != null)
		{
			_animatedSprite = _current.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			_animatedSprite.AnimationFinished += OnAnimationFinished;

			_current.Visible = true;
			GD.Print($"Showing face icon: {_current.Name}");

			if (_animatedSprite != null)
			{
				_animatedSprite.Play("idle");
				GD.Print($"Playing 'idle' animation for {_current.Name}");
			}
			else
			{
				GD.PrintErr($"AnimatedSprite2D not found in {_current.Name}!");
			}
		}
		else
		{
			GD.PrintErr("Current face icon is null!");
		}
	}

	public override void _Ready()
	{
		GD.Print("FaceIconManager started!");

		// Получаем ссылку на игрока
		_player = GetParent()?.GetParent()?.GetNodeOrNull<CharacterBody2D>("Player");
		if (_player == null)
		{
			GD.PrintErr("Player node not found!");
			return;
		}

		// Получаем ссылки на дочерние ноды
		_fullHP = GetNodeOrNull<Node2D>("FaceFullHpIcone");
		_lowDamage = GetNodeOrNull<Node2D>("FaceLowDamageIcone");
		_halfHP = GetNodeOrNull<Node2D>("FaceHalfHpIcone");
		_lowHP = GetNodeOrNull<Node2D>("FaceLowHpIcone");

		// Проверяем, что все иконки найдены
		if (_fullHP == null || _lowDamage == null || _halfHP == null || _lowHP == null)
		{
			GD.PrintErr("One or more face icons are missing!");
			return;
		}

		// Устанавливаем текущий объект
		_current = _fullHP;

		// Скрываем все объекты, кроме текущего
		foreach (var node in new[] { _fullHP, _lowDamage, _halfHP, _lowHP })
		{
			if (node != _current)
			{
				node.Visible = false;
			}
		}

		// Получаем максимальное HP игрока
		_maxHp = PlayerCurrentHp;
		_curHp = _maxHp;
		GD.Print($"Player max HP: {_maxHp}");

		// Получаем AnimatedSprite2D
		_animatedSprite = _current.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_animatedSprite != null)
		{
			_animatedSprite.Play("idle");
			_animatedSprite.AnimationFinished += OnAnimationFinished;
		}
		else
		{
			GD.PrintErr("AnimatedSprite2D not found in current face icon!");
		}
	}

	public override void _Process(double delta)
	{
		if (_player == null)
		{
			GD.PrintErr("Player is null!");
			return;
		}

		// Получаем текущее HP игрока
		int newHp = PlayerCurrentHp;
		if (newHp < _curHp)
		{
			_animatedSprite?.Play("damage");
		}
		_curHp = newHp;
	}

	private void OnAnimationFinished()
	{
		if (_animatedSprite.Animation == "damage")
		{
			GD.Print("Damage finished!");
			double hpPercentage = (double)_curHp / _maxHp;
			SwitchFaceIcon(hpPercentage);
		}
	}
}
