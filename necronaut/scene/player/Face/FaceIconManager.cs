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

	public void SwitchFaceIcon(int condition)
	{
		// Логируем переключение иконки
		GD.Print($"Switching face icon to condition: {condition}");

		// Скрываем текущий объект
		if (_current != null)
		{
			_current.Visible = false;
			GD.Print($"Hiding current face icon: {_current.Name}");
		}

		// Выбираем новый объект в зависимости от условия
		switch (condition)
		{
			case 0:
				_current = _fullHP;
				break;
			case 1:
				_current = _lowDamage;
				break;
			case 2:
				_current = _halfHP;
				break;
			case 3:
				_current = _lowHP;
				break;
			default:
				GD.PrintErr("Invalid condition for face icon!");
				return;
		}
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
		var healPointsVariant = _player.Get("healPoints");
		if (healPointsVariant.VariantType == Variant.Type.Int)
		{
			_maxHp = healPointsVariant.AsInt32();
			_curHp = _maxHp;
			GD.Print($"Player max HP: {_maxHp}");
		}
		else
		{
			GD.PrintErr("Player's healPoints is not an integer or is missing!");
		}

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
		var healPointsVariant = _player.Get("healPoints");
		if (healPointsVariant.VariantType == Variant.Type.Int)
		{
			int newHp = healPointsVariant.AsInt32();
			if (newHp < _curHp)
			{
				_animatedSprite?.Play("damage");
			}
			_curHp = newHp;
		}
		else
		{
			GD.PrintErr("Player's healPoints is not an integer or is missing!");
		}
	}

	private void OnAnimationFinished()
	{
		if (_animatedSprite.Animation == "damage")
		{
			GD.PrintErr("Damage finished!");
			double hpPercentage = (double)_curHp / _maxHp;

			// Логика переключения иконок в зависимости от процента HP
			if (hpPercentage >= 0.75)
			{
				SwitchFaceIcon(0); // Full HP
			}
			else if (hpPercentage >= 0.5)
			{
				SwitchFaceIcon(1); // Low Damage
			}
			else if (hpPercentage >= 0.25)
			{
				SwitchFaceIcon(2); // Half HP
			}
			else
			{
				SwitchFaceIcon(3); // Low HP
			}
		}
	}
}
